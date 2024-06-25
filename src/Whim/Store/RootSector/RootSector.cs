namespace Whim;

/// <inheritdoc/>
public class RootSector : IRootSector, IDisposable
{
	private bool _disposedValue;
	private readonly RootEventListener _listener;

	internal MutableRootSector MutableRootSector { get; }

	/// <inheritdoc/>
	public IMonitorSector MonitorSector => MutableRootSector.MonitorSector;

	/// <inheritdoc/>
	public IWindowSector WindowSector => MutableRootSector.WindowSector;

	/// <inheritdoc/>
	public IMapSector MapSector => MutableRootSector.MapSector;

	/// <inheritdoc/>
	public IWorkspaceSector WorkspaceSector => MutableRootSector.WorkspaceSector;

	internal RootSector(IContext ctx, IInternalContext internalCtx)
	{
		MutableRootSector = new(ctx, internalCtx);
		_listener = new(ctx, internalCtx);
	}

	internal void Initialize()
	{
		MutableRootSector.Initialize();
		_listener.Initialize();
	}

	internal void DispatchEvents() => MutableRootSector.DispatchEvents();

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_listener.Dispose();
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
