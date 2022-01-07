using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
public class MonitorManager : IMonitorManager
{
	private readonly IConfigContext _configContext;

	public Commander Commander { get; } = new();

	/// <summary>
	/// The <see cref="IMonitor"/>s of the computer.
	/// This is initialized to make the compiler happy - we should never have a case where there's
	/// zero <see cref="IMonitor"/>s, as the constructor should throw.
	/// </summary>
	private IMonitor[] _monitors = Array.Empty<IMonitor>();
	private bool disposedValue;

	/// <summary>
	/// Backing field for <see cref="FocusedMonitor"/>.
	/// <b>Please do not use this field directly.</b>
	/// </summary>
	private IMonitor _focusedMonitor;

	/// <summary>
	/// The <see cref="IMonitor"/> which currently has focus.
	/// </summary>
	public IMonitor FocusedMonitor
	{
		get => _focusedMonitor;
		private set
		{
			if (value == _focusedMonitor)
			{
				return;
			}

			IMonitor previousMonitor = _focusedMonitor;
			_focusedMonitor = value;
			MonitorFocused?.Invoke(this, new MonitorFocusedEventArgs(previousMonitor, _focusedMonitor));
		}
	}

	public event EventHandler<MonitorFocusedEventArgs>? MonitorFocused;

	public int Length => _monitors.Length;

	public IEnumerator<IMonitor> GetEnumerator() => _monitors.AsEnumerable().GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	///
	/// </summary>
	/// <exception cref="Exception">
	/// When no monitors are found, or there is no primary monitor.
	/// </exception>
	public MonitorManager(IConfigContext configContext)
	{
		_configContext = configContext;

		// Get the monitors.
		_monitors = GetCurrentMonitors().OrderBy(m => m.X)
										.ThenBy(m => m.Y)
										.ToArray();

		// Get the initial focused monitor
		IMonitor? primaryMonitor = _monitors?.FirstOrDefault(m => m.IsPrimary);
		if (primaryMonitor == null)
		{
			throw new Exception("No primary monitor found.");
		}
		_focusedMonitor = primaryMonitor;

		// Listen for changes in the monitors.
		SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;
	}

	public void Initialize()
	{
		_configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;
	}

	public void WindowManager_WindowFocused(object? sender, WindowEventArgs e)
	{
		Logger.Debug($"Focusing on {e.Window}");
		IMonitor? monitor = _configContext.WorkspaceManager.GetMonitorForWindow(e.Window);

		if (monitor != null)
		{
			FocusedMonitor = monitor;
		}
	}

	private void SystemEvents_DisplaySettingsChanging(object? sender, EventArgs e)
	{
		// Get the new monitors.
		IMonitor[] previousMonitors = _monitors;
		_monitors = GetCurrentMonitors();

		List<IMonitor> unchangedMonitors = new();
		List<IMonitor> removedMonitors = new();
		List<IMonitor> addedMonitors = new();

		HashSet<IMonitor> previousMonitorsSet = new(previousMonitors);
		HashSet<IMonitor> currentMonitorsSet = new(_monitors);

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

		foreach (IMonitor monitor in currentMonitorsSet)
		{
			if (!previousMonitorsSet.Contains(monitor))
			{
				addedMonitors.Add(monitor);
			}
		}

		// Trigger MonitorsChanged event.
		MonitorsChanged?.Invoke(this, new MonitorsChangedEventArgs(unchangedMonitors, removedMonitors, addedMonitors));
	}

	/// <summary>
	/// Gets all the current monitors.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="Exception">When no monitors are found.</exception>
	private static IMonitor[] GetCurrentMonitors()
	{
		Screen[] screens = Screen.AllScreens;

		IMonitor[] _monitors = new IMonitor[screens.Length];
		for (int i = 0; i < screens.Length; i++)
		{
			_monitors[i] = new Monitor(screens[i]);
		}

		if (_monitors.Length == 0)
		{
			throw new Exception("No monitors were found");
		}

		return _monitors;
	}

	public IMonitor GetMonitorAtPoint(int x, int y)
	{
		Logger.Debug($"Getting monitor at point ({x}, {y})");
		Screen screen = Screen.FromPoint(new System.Drawing.Point(x, y));

		IMonitor? monitor = _monitors.FirstOrDefault(m => m.Name == screen.DeviceName);
		if (monitor == null)
		{
			Logger.Error($"No monitor found at point ({x}, {y})");
			return _monitors[0];
		}

		return monitor;
	}

	public IMonitor GetPreviousMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting previous monitor for {monitor}");

		int index = Array.IndexOf(_monitors, monitor) % _monitors.Length;
		if (index == -1)
		{
			Logger.Error($"Monitor {monitor.Name} not found.");
			return _monitors[0];
		}

		return _monitors[(index - 1) % _monitors.Length];
	}

	public IMonitor GetNextMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting next monitor for {monitor}");

		int index = Array.IndexOf(_monitors, monitor) % _monitors.Length;
		if (index == -1)
		{
			Logger.Error($"Monitor {monitor.Name} not found.");
			return _monitors[0];
		}

		return _monitors[(index + 1) % _monitors.Length];
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
