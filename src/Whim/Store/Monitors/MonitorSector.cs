using System;
using System.Collections.Immutable;

namespace Whim;

internal class MonitorSector : SectorBase, IDisposable, IMonitorSector
{
	private readonly IContext _ctx;
	private readonly MonitorEventListener _listener;
	private bool _disposedValue;

	public ImmutableArray<IMonitor> Monitors { get; set; } = ImmutableArray<IMonitor>.Empty;

	public int ActiveMonitorIndex { get; set; } = -1;

	public int PrimaryMonitorIndex { get; set; } = -1;

	public int LastWhimActiveMonitorIndex { get; set; } = -1;

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	public MonitorSector(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_listener = new(ctx, internalCtx);
	}

	/// <inheritdoc/>
	public override void Initialize()
	{
		_ctx.Store.Dispatch(new MonitorsChangedTransform());
		_listener.Initialize();
	}

	/// <inheritdoc/>
	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				case MonitorsChangedEventArgs args:
					MonitorsChanged?.Invoke(this, args);
					break;
			}
		}

		_events.Clear();
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
