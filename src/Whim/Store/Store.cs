using System;
using DotNext;

namespace Whim;

/// <inheritdoc />
public class Store : IStore
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;

	private bool _disposedValue;

	internal readonly RootSector _root;

	/// <inheritdoc />
	public IMonitorSectorEvents MonitorEvents => _root.MutableRootSector.Monitors;

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

	/// <inheritdoc />
	public Result<TResult> Dispatch<TResult>(Transform<TResult> transform)
	{
		Result<TResult> result = transform.Execute(_ctx, _internalCtx, _root.MutableRootSector);
		_root.DispatchEvents();
		return result;
	}

	/// <inheritdoc />
	public TResult Pick<TResult>(Picker<TResult> picker)
	{
		// TODO: reader-writer lock.
		// don't do a read lock if a transform is currently in progress.
		return picker.Execute(_ctx, _internalCtx, _root);
	}

	/// <inheritdoc />
	public TResult Pick<TResult>(PurePicker<TResult> picker)
	{
		// TODO: reader-writer lock
		return picker(_root);
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
