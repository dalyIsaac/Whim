using System;

namespace Whim;

/// <inheritdoc/>
public class RootSector : IRootSector, IDisposable
{
	private bool _disposedValue;

	internal MutableRootSector MutableRootSector { get; }

	/// <inheritdoc/>
	public IMonitorSector Monitors => MutableRootSector.Monitors;

	/// <inheritdoc/>
	public IWorkspaceSector Workspaces => MutableRootSector.Workspaces;

	/// <inheritdoc/>
	public IMapSector Maps => MutableRootSector.Maps;

	/// <inheritdoc/>
	public IWindowSector Windows => MutableRootSector.Windows;

	internal RootSector(IContext ctx, IInternalContext internalCtx)
	{
		MutableRootSector = new MutableRootSector(ctx, internalCtx);
	}

	internal void Initialize() => MutableRootSector.Initialize();

	internal void DispatchEvents() => MutableRootSector.DispatchEvents();

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				MutableRootSector.Dispose();
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
