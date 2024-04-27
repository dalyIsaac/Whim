using System;

namespace Whim;

public class RootSector : IRootSector, IDisposable
{
	private bool _disposedValue;

	private readonly MonitorSector _monitors;

	/// <inheritdoc cref="IMonitorSector"/>
	public IMonitorSector Monitors => _monitors;

	private readonly WorkspaceSector _workspaces;

	/// <inheritdoc cref="WorkspaceSector" />
	public IWorkspaceSector Workspaces => _workspaces;

	private readonly MapSector _maps;

	/// <inheritdoc cref="MapSector" />
	public IMapSector Maps => _maps;

	private readonly WindowSector _windows;

	/// <inheritdoc cref="MapSector" />
	public IWindowSector Windows => _windows;

	internal RootSector(IContext ctx, IInternalContext internalCtx)
	{
		_monitors = new MonitorSector(ctx, internalCtx);
		_workspaces = new WorkspaceSector();
		_maps = new MapSector();
		_windows = new WindowSector();
	}

	internal void Initialize()
	{
		_monitors.Initialize();
		_workspaces.Initialize();
		_maps.Initialize();
		_windows.Initialize();
	}

	internal void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		_monitors.DispatchEvents();
		_workspaces.DispatchEvents();
		_maps.DispatchEvents();
		_windows.DispatchEvents();
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_monitors.Dispose();
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
