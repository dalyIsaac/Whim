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
	private readonly IInternalContext _internalContext;

	private bool _disposedValue;

	public IMonitor ActiveMonitor => _context.Store.Select(new GetActiveMonitor());

	public IMonitor PrimaryMonitor => _context.Store.Select(new GetPrimaryMonitor());

	public IMonitor LastWhimActiveMonitor => _context.Store.Select(new GetLastWhimActiveMonitor());

	public int Length => _context.Store.Select(new GetAllMonitors()).Count;

	public IEnumerator<IMonitor> GetEnumerator() => _context.Store.Select(new GetAllMonitors()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	/// Creates a new instance of <see cref="MonitorManager"/>.
	/// </summary>
	/// <exception cref="Exception">
	/// When no monitors are found, or there is no primary monitor.
	/// </exception>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	public MonitorManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public void Initialize()
	{
		_context.Store.Dispatch(new MonitorsChangedTransform());
		_context.Store.MonitorSlice.MonitorsChanged += MonitorSlice_MonitorsChanged;
	}

	private void MonitorSlice_MonitorsChanged(object? sender, MonitorsChangedEventArgs e) =>
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
		return _context.Store.Select(new GetMonitorAtPointSelector(point, GetFirst: true))!;
	}

	public IMonitor GetPreviousMonitor(IMonitor monitor)
	{
		return _context.Store.Select(new GetPreviousMonitorSelector(monitor));
	}

	public IMonitor GetNextMonitor(IMonitor monitor)
	{
		return _context.Store.Select(new GetNextMonitorSelector(monitor));
	}

	// TODO: Remove when removing butler.
	public IMonitor? GetMonitorByHandle(HMONITOR hmonitor) =>
		_context.Store.Select(new GetAllMonitors()).FirstOrDefault(m => m.Handle == hmonitor);

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing monitor manager");
				_internalContext.Dispose();
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
