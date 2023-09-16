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
	private readonly ICoreNativeManager _coreNativeManager;
	private readonly IMouseHook _mouseHook;
	private readonly IWindowMessageMonitor _windowMessageMonitor;

	/// <summary>
	/// The <see cref="IMonitor"/>s of the computer.
	/// This is initialized to make the compiler happy - we should never have a case where there's
	/// zero <see cref="IMonitor"/>s, as the constructor should throw.
	/// </summary>
	private Monitor[] _monitors = Array.Empty<Monitor>();
	private bool _disposedValue;

	/// <summary>
	/// The <see cref="IMonitor"/> which currently has focus.
	/// </summary>
	public IMonitor ActiveMonitor { get; private set; }

	public IMonitor PrimaryMonitor { get; private set; }

	public int Length => _monitors.Length;

	public IEnumerator<IMonitor> GetEnumerator() => _monitors.AsEnumerable().GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	/// Creates a new instance of <see cref="MonitorManager"/>.
	/// </summary>
	/// <exception cref="Exception">
	/// When no monitors are found, or there is no primary monitor.
	/// </exception>
	public MonitorManager(
		IContext context,
		ICoreNativeManager coreNativeManager,
		IMouseHook mouseHook,
		IWindowMessageMonitor? windowMessageMonitor = null
	)
	{
		_context = context;
		_coreNativeManager = coreNativeManager;
		_mouseHook = mouseHook;
		_windowMessageMonitor = windowMessageMonitor ?? new WindowMessageMonitor(_context, _coreNativeManager);

		// Get the monitors.
		_monitors = GetCurrentMonitors();

		// Get the initial active monitor
		IMonitor? primaryMonitor =
			(_monitors?.FirstOrDefault(m => m.IsPrimary)) ?? throw new Exception("No primary monitor found.");
		ActiveMonitor = primaryMonitor;
		PrimaryMonitor = primaryMonitor;
	}

	public void Initialize()
	{
		_windowMessageMonitor.DisplayChanged += WindowMessageMonitor_MonitorsChanged;
		_windowMessageMonitor.WorkAreaChanged += WindowMessageMonitor_MonitorsChanged;
		_windowMessageMonitor.DpiChanged += WindowMessageMonitor_MonitorsChanged;
		_windowMessageMonitor.SessionChanged += WindowMessageMonitor_SessionChanged;
		_mouseHook.MouseLeftButtonUp += MouseHook_MouseLeftButtonUp;
	}

	public void WindowFocused(IWindow? window)
	{
		Logger.Debug($"Focusing on {window}");

		HWND hwnd = window?.Handle ?? _coreNativeManager.GetForegroundWindow();
		HMONITOR hMONITOR = _coreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

		foreach (Monitor monitor in _monitors)
		{
			if (monitor._hmonitor.Equals(hMONITOR))
			{
				Logger.Debug($"Setting active monitor to {monitor}");

				ActiveMonitor = monitor;
				break;
			}
		}
	}

	private void WindowMessageMonitor_SessionChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		// If we update monitors too quickly, the reported working area can sometimes be the
		// monitor's bounds, which is incorrect. So, we wait a bit before updating the monitors.
		// This gives Windows some to figure out the correct working area.
		_coreNativeManager.TryEnqueue(async () =>
		{
			await Task.Delay(5000).ConfigureAwait(true);
			WindowMessageMonitor_MonitorsChanged(sender, e);
		});
	}

	private void WindowMessageMonitor_MonitorsChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		Logger.Debug($"Monitors changed: {e.MessagePayload}");

		// Get the new monitors.
		IMonitor[] previousMonitors = _monitors;
		_monitors = GetCurrentMonitors();

		List<IMonitor> unchangedMonitors = new();
		List<IMonitor> removedMonitors = new();
		List<IMonitor> addedMonitors = new();

		HashSet<IMonitor> previousMonitorsSet = new(previousMonitors);
		HashSet<IMonitor> currentMonitorsSet = new(_monitors);

		// For each monitor in the previous set, check if it's in the current set.
		foreach (IMonitor monitor in previousMonitorsSet)
		{
			if (currentMonitorsSet.Contains(monitor))
			{
				unchangedMonitors.Add(monitor);
			}
			else
			{
				removedMonitors.Add(monitor);
			}
		}

		// For each monitor in the current set, check if it's in the previous set.
		foreach (IMonitor monitor in currentMonitorsSet)
		{
			if (!previousMonitorsSet.Contains(monitor))
			{
				addedMonitors.Add(monitor);
			}

			if (monitor.IsPrimary)
			{
				PrimaryMonitor = monitor;
			}
		}

		// Notify listeners of the unchanged, removed, and added monitors.
		MonitorsChanged?.Invoke(
			this,
			new MonitorsChangedEventArgs()
			{
				UnchangedMonitors = unchangedMonitors,
				RemovedMonitors = removedMonitors,
				AddedMonitors = addedMonitors
			}
		);
	}

	private void MouseHook_MouseLeftButtonUp(object? sender, MouseEventArgs e)
	{
		IMonitor monitor = GetMonitorAtPoint(e.Point);
		Logger.Debug($"Mouse left button up on {monitor}");
		ActiveMonitor = monitor;
	}

	private HMONITOR GetPrimaryHMonitor()
	{
		return _coreNativeManager.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
	}

	/// <summary>
	/// Gets all the current monitors.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="Exception">When no monitors are found.</exception>
	private unsafe Monitor[] GetCurrentMonitors()
	{
		List<HMONITOR> hmonitors = new();
		HMONITOR primaryHMonitor = GetPrimaryHMonitor();

		if (_coreNativeManager.HasMultipleMonitors())
		{
			MonitorEnumCallback closure = new();
			MONITORENUMPROC proc = new(closure.Callback);

			_coreNativeManager.EnumDisplayMonitors(null, null, proc, (LPARAM)0);

			hmonitors = closure.Monitors;
		}

		if (hmonitors.Count == 0)
		{
			hmonitors.Add(primaryHMonitor);
		}

		Monitor[] currentMonitors = new Monitor[hmonitors.Count];
		for (int i = 0; i < currentMonitors.Length; i++)
		{
			HMONITOR hmonitor = hmonitors[i];
			bool isPrimaryHMonitor = hmonitor == primaryHMonitor;

			// Try find the monitor in the list of existing monitors. If we can find it, update
			// its properties.
			Monitor? monitor = _monitors.FirstOrDefault(m => m._hmonitor == hmonitor);

			if (monitor is null)
			{
				monitor = new Monitor(_coreNativeManager, hmonitor, isPrimaryHMonitor);
			}
			else
			{
				monitor.Update(isPrimaryHMonitor);
			}

			currentMonitors[i] = monitor;
		}

		return currentMonitors.OrderBy(m => m.WorkingArea.X).ThenBy(m => m.WorkingArea.Y).ToArray();
	}

	public IMonitor GetMonitorAtPoint(IPoint<int> point)
	{
		Logger.Debug($"Getting monitor at point {point}");
		HMONITOR hmonitor = _coreNativeManager.MonitorFromPoint(
			point.ToSystemPoint(),
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		IMonitor? monitor = _monitors.FirstOrDefault(m => m._hmonitor == hmonitor);
		if (monitor == null)
		{
			Logger.Error($"No monitor found at point {point}");
			return _monitors[0];
		}

		return monitor;
	}

	public IMonitor GetPreviousMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting previous monitor for {monitor}");

		int index = Array.IndexOf(_monitors, monitor);
		if (index == -1)
		{
			Logger.Error($"Monitor {monitor} not found.");
			return _monitors[0];
		}

		return _monitors[(index - 1).Mod(_monitors.Length)];
	}

	public IMonitor GetNextMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting next monitor for {monitor}");

		int index = Array.IndexOf(_monitors, monitor);
		if (index == -1)
		{
			Logger.Error($"Monitor {monitor} not found.");
			return _monitors[0];
		}

		return _monitors[(index + 1).Mod(_monitors.Length)];
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing monitor manager");

				// dispose managed state (managed objects)
				_windowMessageMonitor.DisplayChanged -= WindowMessageMonitor_MonitorsChanged;
				_windowMessageMonitor.Dispose();
				_mouseHook.MouseLeftButtonUp -= MouseHook_MouseLeftButtonUp;
				_mouseHook.Dispose();
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
