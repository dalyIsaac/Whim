using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
internal class MonitorManager : IMonitorManager
{
	private readonly IConfigContext _configContext;

	/// <summary>
	/// The <see cref="IMonitor"/>s of the computer.
	/// This is initialized to make the compiler happy - we should never have a case where there's
	/// zero <see cref="IMonitor"/>s, as the constructor should throw.
	/// </summary>
	private IMonitor[] _monitors = Array.Empty<IMonitor>();
	private bool _disposedValue;

	/// <summary>
	/// The <see cref="IMonitor"/> which currently has focus.
	/// </summary>
	public IMonitor FocusedMonitor { get; private set; }

	public IMonitor PrimaryMonitor { get; private set; }

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
		FocusedMonitor = primaryMonitor;
		PrimaryMonitor = primaryMonitor;
	}

	public void Initialize()
	{
		// Listen for changes in the monitors.
		SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;
	}

	/// <summary>
	/// Called when the window has been focused.
	/// </summary>
	/// <param name="window"></param>
	internal void WindowFocused(IWindow window)
	{
		Logger.Debug($"Focusing on {window}");
		IMonitor? monitor = _configContext.WorkspaceManager.GetMonitorForWindow(window);

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

			if (monitor.IsPrimary)
			{
				PrimaryMonitor = monitor;
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

	public IMonitor GetMonitorAtPoint(IPoint<int> point)
	{
		Logger.Debug($"Getting monitor at point {point}");
		Screen screen = Screen.FromPoint(point);

		IMonitor? monitor = _monitors.FirstOrDefault(m => m.Name == screen.DeviceName);
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
				SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
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
