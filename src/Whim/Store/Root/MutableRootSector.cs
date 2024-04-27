using System;

namespace Whim;

internal class MutableRootSector : IDisposable
{
	private bool _disposedValue;

	public MonitorSector Monitors { get; }
	public WindowSector Windows { get; }
	public WorkspaceSector Workspaces { get; }
	public MapSector Maps { get; }

	public MutableRootSector(IContext ctx, IInternalContext internalCtx)
	{
		Monitors = new MonitorSector(ctx, internalCtx);
		Windows = new WindowSector(ctx, internalCtx);
		Workspaces = new WorkspaceSector();
		Maps = new MapSector();
	}

	public void Initialize()
	{
		Monitors.Initialize();
		Windows.Initialize();
		Workspaces.Initialize();
		Maps.Initialize();
	}

	public void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		Monitors.DispatchEvents();
		Windows.DispatchEvents();
		Workspaces.DispatchEvents();
		Maps.DispatchEvents();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				Monitors.Dispose();
				Windows.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
