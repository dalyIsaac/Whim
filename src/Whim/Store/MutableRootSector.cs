using System;

namespace Whim;

internal class MutableRootSector : IDisposable
{
	private bool _disposedValue;

	public MonitorSector Monitors { get; }
	public WorkspaceSector Workspaces { get; }
	public MapSector Maps { get; }
	public WindowSector Windows { get; }

	public MutableRootSector(IContext ctx, IInternalContext internalCtx)
	{
		Monitors = new MonitorSector(ctx, internalCtx);
		Workspaces = new WorkspaceSector();
		Maps = new MapSector();
		Windows = new WindowSector();
	}

	public void Initialize()
	{
		Monitors.Initialize();
		Workspaces.Initialize();
		Maps.Initialize();
		Windows.Initialize();
	}

	public void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		Monitors.DispatchEvents();
		Workspaces.DispatchEvents();
		Maps.DispatchEvents();
		Windows.DispatchEvents();
	}

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

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
