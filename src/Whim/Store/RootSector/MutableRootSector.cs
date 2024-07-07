namespace Whim;

internal class MutableRootSector : SectorBase, IDisposable
{
	private readonly IContext _ctx;
	private readonly object _lock = new();
	private bool _disposedValue;
	private bool _dispatchEventsScheduled;

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
		_ctx = ctx;
		MonitorSector = new MonitorSector(ctx, internalCtx);
		WindowSector = new WindowSector(ctx, internalCtx);
		WorkspaceSector = new WorkspaceSector(ctx, internalCtx);
		MapSector = new MapSector();
	}

	public override void Initialize()
	{
		Logger.Information("Initializing MutableRootSector");
		MonitorSector.Initialize();
		WindowSector.Initialize();
		WorkspaceSector.Initialize();
		MapSector.Initialize();
	}

	public override void DispatchEvents()
	{
		lock (_lock)
		{
			if (_dispatchEventsScheduled)
			{
				Logger.Debug("Dispatch events already scheduled");
				return;
			}

			if (HasQueuedEvents)
			{
				Logger.Debug("Queued events detected, scheduling dispatch");
				_dispatchEventsScheduled = true;
				_ctx.NativeManager.TryEnqueue(DispatchEventsFn);
			}
		}
	}

	private void DispatchEventsFn()
	{
		// We can't lock here because of the STA, so we set the flag to false before we start
		// dispatching events.
		_dispatchEventsScheduled = false;

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
