using System;
using System.Threading;
using System.Threading.Tasks;
using DotNext;

namespace Whim;

/// <inheritdoc />
public class Store : IStore
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;
	private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

	private bool _disposedValue;

	internal readonly RootSector _root;

	/// <inheritdoc />
	public IMonitorSectorEvents MonitorEvents => _root.MutableRootSector.MonitorSector;

	/// <inheritdoc />
	public IWindowSectorEvents WindowEvents => _root.MutableRootSector.WindowSector;

	internal Store(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;

		_root = new RootSector(ctx, internalCtx);
	}

	/// <inheritdoc />
	public void Initialize()
	{
		_root.Initialize();
	}

	private Result<TResult> DispatchFn<TResult>(Transform<TResult> transform) =>
		transform.Execute(_ctx, _internalCtx, _root.MutableRootSector);

	/// <inheritdoc />
	public Result<TResult> Dispatch<TResult>(Transform<TResult> transform)
	{
		if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
		{
			return Task.Run(() =>
			{
				try
				{
					_lock.EnterWriteLock();
					return DispatchFn(transform);
				}
				finally
				{
					_lock.ExitWriteLock();
					_ctx.NativeManager.TryEnqueue(_root.DispatchEvents);
				}
			}).Result;
		}

		return DispatchFn(transform);
	}

	private TResult PickFn<TResult>(Picker<TResult> picker) => picker.Execute(_ctx, _internalCtx, _root);

	/// <inheritdoc />
	public TResult Pick<TResult>(Picker<TResult> picker)
	{
		if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
		{
			return Task.Run(() =>
			{
				try
				{
					_lock.EnterReadLock();
					return PickFn(picker);
				}
				finally
				{
					_lock.ExitReadLock();
					_ctx.NativeManager.TryEnqueue(_root.DispatchEvents);
				}
			}).Result;
		}

		return PickFn(picker);
	}

	private TResult PurePickFn<TResult>(PurePicker<TResult> picker) => picker(_root);

	/// <inheritdoc />
	public TResult Pick<TResult>(PurePicker<TResult> picker)
	{
		if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
		{
			return Task.Run(() =>
			{
				try
				{
					_lock.EnterReadLock();
					return PurePickFn(picker);
				}
				finally
				{
					_lock.ExitReadLock();
					_ctx.NativeManager.TryEnqueue(_root.DispatchEvents);
				}
			}).Result;
		}

		return PurePickFn(picker);
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_root.Dispose();
				_lock.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
