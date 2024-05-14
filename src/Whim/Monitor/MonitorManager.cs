using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
internal class MonitorManager : IInternalMonitorManager, IMonitorManager
{
	private readonly IContext _context;

	private bool _disposedValue;

	public IMonitor ActiveMonitor => _context.Store.Pick(Pickers.PickActiveMonitor());

	public IMonitor PrimaryMonitor => _context.Store.Pick(Pickers.PickPrimaryMonitor());

	public IMonitor LastWhimActiveMonitor => _context.Store.Pick(Pickers.PickLastWhimActiveMonitor());

	public int Length => _context.Store.Pick(Pickers.PickAllMonitors()).Count;

	public IEnumerator<IMonitor> GetEnumerator() => _context.Store.Pick(Pickers.PickAllMonitors()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	/// Creates a new instance of <see cref="MonitorManager"/>.
	/// </summary>
	/// <exception cref="Exception">
	/// When no monitors are found, or there is no primary monitor.
	/// </exception>
	/// <param name="context"></param>
	public MonitorManager(IContext context)
	{
		_context = context;
	}

	public void Initialize()
	{
		_context.Store.MonitorEvents.MonitorsChanged += MonitorSector_MonitorsChanged;
	}

	private void MonitorSector_MonitorsChanged(object? sender, MonitorsChangedEventArgs e) =>
		MonitorsChanged?.Invoke(sender, e);

	public void ActivateEmptyMonitor(IMonitor monitor) =>
		_context.Store.Dispatch(new ActivateEmptyMonitorTransform(monitor.Handle));

	public IMonitor GetMonitorAtPoint(IPoint<int> point) =>
		_context.Store.Pick(Pickers.PickMonitorAtPoint(point, getFirst: true)).Value;

	public IMonitor GetPreviousMonitor(IMonitor monitor) =>
		_context.Store.Pick(Pickers.PickAdjacentMonitor(monitor.Handle, reverse: true, getFirst: true)).Value;

	public IMonitor GetNextMonitor(IMonitor monitor) =>
		_context.Store.Pick(Pickers.PickAdjacentMonitor(monitor.Handle, reverse: false, getFirst: true)).Value;

	public IMonitor? GetMonitorByHandle(HMONITOR hmonitor) =>
		_context.Store.Pick(Pickers.PickMonitorByHandle(hmonitor)).OrDefault();

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing monitor manager");

				// dispose managed state (managed objects)
				_context.Store.MonitorEvents.MonitorsChanged -= MonitorSector_MonitorsChanged;
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
