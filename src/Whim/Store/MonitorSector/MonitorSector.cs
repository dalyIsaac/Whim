namespace Whim;

internal class MonitorSector : SectorBase, IDisposable, IMonitorSector, IMonitorSectorEvents
{
	private readonly IContext _ctx;
	private readonly MonitorEventListener _listener;
	private bool _disposedValue;

	public int MonitorsChangingTasks { get; set; }
	public int MonitorsChangedDelay { get; set; } = 3 * 1000;
	public ImmutableArray<IMonitor> Monitors { get; set; } = ImmutableArray<IMonitor>.Empty;
	public HMONITOR ActiveMonitorHandle { get; set; }
	public HMONITOR PrimaryMonitorHandle { get; set; }
	public HMONITOR LastWhimActiveMonitorHandle { get; set; }

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	public MonitorSector(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_listener = new(ctx, internalCtx);
	}

	public override void Initialize()
	{
		_ctx.Store.Dispatch(new MonitorsChangedTransform());
		_listener.Initialize();
	}

	public override void DispatchEvents()
	{
		// Use index access to prevent the list from being modified during enumeration.
		for (int idx = 0; idx < _events.Count; idx++)
		{
			EventArgs eventArgs = _events[idx];
			switch (eventArgs)
			{
				case MonitorsChangedEventArgs args:
					MonitorsChanged?.Invoke(this, args);
					break;
			}
		}

		_events.Clear();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_listener.Dispose();
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
