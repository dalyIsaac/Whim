using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
internal class MonitorManager : IMonitorManager
{
	private readonly IConfigContext _configContext;
	private readonly ICoreNativeManager _coreNativeManager;
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
	public MonitorManager(
		IConfigContext configContext,
		ICoreNativeManager coreNativeManager,
		IWindowMessageMonitor? windowMessageMonitor = null
	)
	{
		_configContext = configContext;
		_coreNativeManager = coreNativeManager;
		_windowMessageMonitor = windowMessageMonitor ?? new WindowMessageMonitor(_configContext, _coreNativeManager);

		// Get the monitors.
		_monitors = GetCurrentMonitors().OrderBy(m => m.Bounds.X).ThenBy(m => m.Bounds.Y).ToArray();

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
		_windowMessageMonitor.DisplayChanged += WindowMessageMonitor_DisplayChanged;
		_windowMessageMonitor.WorkAreaChanged += WindowMessageMonitor_WorkAreaChanged;
		_windowMessageMonitor.DpiChanged += WindowMessageMonitor_DpiChanged;
	}

	/// <summary>
	/// Called when the window has been focused.
	/// </summary>
	/// <param name="window"></param>
	internal virtual void WindowFocused(IWindow window)
	{
		Logger.Debug($"Focusing on {window}");
		IMonitor? monitor = _configContext.WorkspaceManager.GetMonitorForWindow(window);

		if (monitor != null)
		{
			FocusedMonitor = monitor;
		}
	}

	private void WindowMessageMonitor_DisplayChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		Logger.Debug($"Display changed: {e.MessagePayload}");

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

	private void WindowMessageMonitor_WorkAreaChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		Logger.Debug($"Work area changed: {e.MessagePayload}");

		MonitorsChanged?.Invoke(
			this,
			new MonitorsChangedEventArgs()
			{
				UnchangedMonitors = _monitors,
				RemovedMonitors = Array.Empty<IMonitor>(),
				AddedMonitors = Array.Empty<IMonitor>()
			}
		);
	}

	private void WindowMessageMonitor_DpiChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		Logger.Debug($"DPI changed: {e.MessagePayload}");

		MonitorsChanged?.Invoke(
			this,
			new MonitorsChangedEventArgs()
			{
				UnchangedMonitors = _monitors,
				RemovedMonitors = Array.Empty<IMonitor>(),
				AddedMonitors = Array.Empty<IMonitor>()
			}
		);
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
		List<HMONITOR> hmonitors;
		HMONITOR primaryHMonitor = GetPrimaryHMonitor();

		if (_coreNativeManager.HasMultipleMonitors())
		{
			MonitorEnumCallback closure = new();
			MONITORENUMPROC proc = new(closure.Callback);

			_coreNativeManager.EnumDisplayMonitors(null, null, proc, (LPARAM)0);

			if (closure.Monitors.Count > 0)
			{
				hmonitors = closure.Monitors;
			}
			else
			{
				hmonitors = new List<HMONITOR>() { primaryHMonitor };
			}
		}
		else
		{
			hmonitors = new List<HMONITOR>() { primaryHMonitor };
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

		return currentMonitors;
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
				_windowMessageMonitor.DisplayChanged -= WindowMessageMonitor_DisplayChanged;
				_windowMessageMonitor.Dispose();
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
