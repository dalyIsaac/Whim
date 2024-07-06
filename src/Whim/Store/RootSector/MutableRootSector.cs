namespace Whim;

internal class MutableRootSector : SectorBase, IDisposable
{
	private bool _disposedValue;

	public MonitorSector MonitorSector { get; }
	public WindowSector WindowSector { get; }
	public WorkspaceSector WorkspaceSector { get; }
	public MapSector MapSector { get; }

	/// <inheritdoc/>
	public override bool HasQueuedEvents =>
		MonitorSector.HasQueuedEvents
		|| WindowSector.HasQueuedEvents
		|| WorkspaceSector.HasQueuedEvents
		|| MapSector.HasQueuedEvents;

	public MutableRootSector(IContext ctx, IInternalContext internalCtx)
	{
		MonitorSector = new MonitorSector(ctx, internalCtx);
		WindowSector = new WindowSector(ctx, internalCtx);
		WorkspaceSector = new WorkspaceSector(ctx, internalCtx);
		MapSector = new MapSector();
	}

	public override void Initialize()
	{
		MonitorSector.Initialize();
		WindowSector.Initialize();
		WorkspaceSector.Initialize();
		MapSector.Initialize();
	}

	public override void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		MonitorSector.DispatchEvents();
		WindowSector.DispatchEvents();
		WorkspaceSector.DispatchEvents();
		MapSector.DispatchEvents();
	}

	public void DoLayout() => WorkspaceSector.DoLayout();

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				MonitorSector.Dispose();
				WindowSector.Dispose();
				WorkspaceSector.Dispose();
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
