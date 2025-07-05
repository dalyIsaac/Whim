using System.Collections;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="MonitorManager"/>.
/// </remarks>
/// <exception cref="Exception">
/// When no monitors are found, or there is no primary monitor.
/// </exception>
/// <param name="context"></param>
internal class MonitorManager(IContext context) : IMonitorManager
{
	private readonly IContext _ctx = context;

	private bool _disposedValue;

	public IMonitor ActiveMonitor => _ctx.Store.Pick(PickActiveMonitor());

	public IMonitor PrimaryMonitor => _ctx.Store.Pick(PickPrimaryMonitor());

	public IMonitor LastWhimActiveMonitor => _ctx.Store.Pick(PickLastWhimActiveMonitor());

	public int Length => _ctx.Store.Pick(PickAllMonitors()).Count;

	public IEnumerator<IMonitor> GetEnumerator() => _ctx.Store.Pick(PickAllMonitors()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	public void Initialize()
	{
		_ctx.Store.MonitorEvents.MonitorsChanged += MonitorSector_MonitorsChanged;
	}

	private void MonitorSector_MonitorsChanged(object? sender, MonitorsChangedEventArgs e) =>
		MonitorsChanged?.Invoke(sender, e);

	public void ActivateEmptyMonitor(IMonitor monitor) =>
		_ctx.Store.Dispatch(new ActivateEmptyMonitorTransform(monitor.Handle));

	public IMonitor GetMonitorAtPoint(IPoint<int> point) =>
		_ctx.Store.Pick(PickMonitorAtPoint(point, getFirst: true)).Value!;

	public IMonitor GetPreviousMonitor(IMonitor monitor) =>
		_ctx.Store.Pick(PickAdjacentMonitor(monitor.Handle, reverse: true, getFirst: true)).Value!;

	public IMonitor GetNextMonitor(IMonitor monitor) =>
		_ctx.Store.Pick(PickAdjacentMonitor(monitor.Handle, reverse: false, getFirst: true)).Value!;

	public IMonitor? GetMonitorByHandle(HMONITOR hmonitor) =>
		_ctx.Store.Pick(PickMonitorByHandle(hmonitor)).ValueOrDefault;

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing monitor manager");

				// dispose managed state (managed objects)
				_ctx.Store.MonitorEvents.MonitorsChanged -= MonitorSector_MonitorsChanged;
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
