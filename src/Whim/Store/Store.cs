using System;
using DotNext;

namespace Whim;

/// <inheritdoc />
public class Store : IStore
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;
	private bool _disposedValue;

	/// <inheritdoc />
	public MonitorSector Monitors { get; }

	/// <inheritdoc />
	public WorkspaceSector Workspaces { get; }

	/// <inheritdoc />
	public MapSector Maps { get; }

	/// <inheritdoc />
	public WindowSector Windows { get; }

	internal Store(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;

		Monitors = new MonitorSector(ctx, internalCtx);
		Workspaces = new WorkspaceSector();
		Maps = new MapSector();
		Windows = new WindowSector();
	}

	/// <inheritdoc />
	public void Initialize()
	{
		Monitors.Initialize();
		Workspaces.Initialize();
		Maps.Initialize();
		Windows.Initialize();
	}

	/// <inheritdoc />
	public void Dispatch(Transform transform)
	{
		// TODO: reader-writer lock.
		transform.Execute(_ctx, _internalCtx);
		DispatchEvents();
	}

	/// <inheritdoc />
	public Result<TResult> Dispatch<TResult>(Transform<TResult> transform)
	{
		Result<TResult> result = transform.Execute(_ctx, _internalCtx);
		DispatchEvents();
		return result;
	}

	private void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		Monitors.DispatchEvents();
		Workspaces.DispatchEvents();
		Maps.DispatchEvents();
		Windows.DispatchEvents();
	}

	/// <inheritdoc cref="Windows" />
	public TResult Pick<TResult>(Picker<TResult> picker)
	{
		// TODO: reader-writer lock.
		// don't do a read lock if a transform is currently in progress.
		return picker.Execute(_ctx, _internalCtx);
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				Monitors.Dispose();
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
