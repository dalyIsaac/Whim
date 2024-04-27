using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
internal class MonitorManager : IInternalMonitorManager, IMonitorManager
{
	private readonly IContext _context;
	private bool _disposedValue;

	public IMonitor ActiveMonitor => _context.Store.Pick(new GetActiveMonitorPicker());

	public IMonitor PrimaryMonitor => _context.Store.Pick(new GetPrimaryMonitorPicker());

	public IMonitor LastWhimActiveMonitor => _context.Store.Pick(new GetLastWhimActiveMonitorPicker());

	public int Length => _context.Store.Pick(new GetAllMonitorsPicker()).Count;

	public IEnumerator<IMonitor> GetEnumerator() => _context.Store.Pick(new GetAllMonitorsPicker()).GetEnumerator();

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
		_context.Store.Monitors.MonitorsChanged += MonitorSector_MonitorsChanged;
	}

	private void MonitorSector_MonitorsChanged(object? sender, MonitorsChangedEventArgs e) =>
		MonitorsChanged?.Invoke(sender, e);

	public void OnWindowFocused(IWindow? window)
	{
		_context.Store.Dispatch(new WindowFocusedTransform(window));
	}

	public void ActivateEmptyMonitor(IMonitor monitor)
	{
		_context.Store.Dispatch(new ActivateEmptyMonitorTransform(monitor));
	}

	public IMonitor GetMonitorAtPoint(IPoint<int> point)
	{
		return _context.Store.Pick(new GetMonitorAtPointPicker(point, GetFirst: true)).Value;
	}

	public IMonitor GetPreviousMonitor(IMonitor monitor)
	{
		return _context.Store.Pick(new GetPreviousMonitorPicker(monitor, GetFirst: true)).Value;
	}

	public IMonitor GetNextMonitor(IMonitor monitor)
	{
		return _context.Store.Pick(new GetNextMonitorPicker(monitor, GetFirst: true)).Value;
	}

	// TODO: Remove when removing butler.
	public IMonitor? GetMonitorByHandle(HMONITOR hmonitor) =>
		_context.Store.Pick(new GetAllMonitorsPicker()).FirstOrDefault(m => m.Handle == hmonitor);

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)ector
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.Store.MonitorSector.MonitorsChanged -= MonitorSector_MonitorsChanged;
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
