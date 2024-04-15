using System;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The slice containing monitors.
/// </summary>
public class MonitorSlice : ISlice, IDisposable
{
	private readonly MonitorEventListener _listener;
	private bool _disposedValue;

	/// <summary>
	/// All the monitors currently tracked by Whim.
	/// </summary>
	internal ImmutableArray<IMonitor> Monitors { get; set; } = ImmutableArray<IMonitor>.Empty;

	/// <summary>
	/// The index of the monitor which is currently active, in <see cref="Monitors"/>.
	/// </summary>
	internal int ActiveMonitorIndex { get; set; } = -1;

	/// <summary>
	/// The index of the primary monitor, in <see cref="Monitors"/>.
	/// </summary>
	internal int PrimaryMonitorIndex { get; set; } = -1;

	/// <summary>
	/// The index of the last monitor which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	internal int LastWhimActiveMonitorIndex { get; set; } = -1;

	/// <summary>
	/// Event raised when the monitors handled by Whim are changed.
	/// </summary>
	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	internal MonitorSlice(IContext ctx, IInternalContext internalCtx)
	{
		_listener = new(ctx, internalCtx);
	}

	/// <inheritdoc/>
	internal override void Initialize()
	{
		_listener.Initialize();
	}

	/// <inheritdoc/>
	internal override void DispatchEvents()
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
