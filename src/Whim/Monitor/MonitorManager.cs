using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
internal class MonitorManager : IInternalMonitorManager, IMonitorManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	/// <summary>
	/// The <see cref="IMonitor"/>s of the computer.
	/// This is initialized to make the compiler happy - we should never have a case where there's
	/// zero <see cref="IMonitor"/>s, as the constructor should throw.
	/// </summary>
	private Monitor[] _monitors = Array.Empty<Monitor>();
	private bool _disposedValue;

	public IMonitor ActiveMonitor => _context.Store.Pick(Pickers.GetActiveMonitor());

	public IMonitor PrimaryMonitor => _context.Store.Pick(Pickers.GetPrimaryMonitor());

	public IMonitor LastWhimActiveMonitor => _context.Store.Pick(Pickers.GetLastWhimActiveMonitor());

	public int Length => _context.Store.Pick(Pickers.GetAllMonitors()).Count;

	public IEnumerator<IMonitor> GetEnumerator() => _context.Store.Pick(Pickers.GetAllMonitors()).GetEnumerator();

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

		// Get the monitors.
		_monitors = GetCurrentMonitors();

		// Get the initial active monitor
		IMonitor? primaryMonitor =
			(_monitors?.FirstOrDefault(m => m.IsPrimary)) ?? throw new Exception("No primary monitor found.");
		ActiveMonitor = primaryMonitor;
		PrimaryMonitor = primaryMonitor;
		LastWhimActiveMonitor = primaryMonitor;
	}

	public void Initialize()
	{
		_context.Store.MonitorEvents.MonitorsChanged += MonitorSector_MonitorsChanged;
		_internalContext.MouseHook.MouseLeftButtonUp += MouseHook_MouseLeftButtonUp;
	}

	private void MonitorSector_MonitorsChanged(object? sender, MonitorsChangedEventArgs e) =>
		MonitorsChanged?.Invoke(sender, e);

	public void ActivateEmptyMonitor(IMonitor monitor) =>
		_context.Store.Dispatch(new ActivateEmptyMonitorTransform(monitor.Handle));

	private void MouseHook_MouseLeftButtonUp(object? sender, MouseEventArgs e)
	{
		IMonitor monitor = GetMonitorAtPoint(e.Point);
		Logger.Debug($"Mouse left button up on {monitor}");
		ActiveMonitor = monitor;
	}

	public IMonitor GetMonitorAtPoint(IPoint<int> point) => _context.Store.Pick(Pickers.GetMonitorAtPoint(point)).Value;

	public IMonitor GetPreviousMonitor(IMonitor monitor) =>
		_context.Store.Pick(Pickers.GetAdjacentMonitor(monitor.Handle, reverse: false, getFirst: true)).Value;

	public IMonitor GetNextMonitor(IMonitor monitor) =>
		_context.Store.Pick(Pickers.GetAdjacentMonitor(monitor.Handle, reverse: false, getFirst: true)).Value;

	public IMonitor? GetMonitorByHandle(HMONITOR hmonitor) => _monitors.FirstOrDefault(m => m.Handle == hmonitor);

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing monitor manager");

				// dispose managed state (managed objects)
				_context.Store.MonitorEvents.MonitorsChanged -= MonitorSector_MonitorsChanged;

				_internalContext.MouseHook.MouseLeftButtonUp -= MouseHook_MouseLeftButtonUp;
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
