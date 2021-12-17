using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Whim.Core;

/// <summary>
/// Implementation of <see cref="IMonitorManager"/>.
/// </summary>
public class MonitorManager : IMonitorManager
{
	private readonly IConfigContext _configContext;

	public Commander Commander { get; } = new();

	/// <summary>
	/// The <see cref="IMonitor"/>s of the computer.
	/// </summary>
	private IMonitor[] _monitors;

	/// <summary>
	/// The <see cref="IMonitor"/> which currently has focus.
	/// </summary>
	public IMonitor FocusedMonitor { get; private set; }

	public int Length => _monitors.Length;

	public IEnumerator<IMonitor> GetEnumerator() => _monitors.AsEnumerable().GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	///
	/// </summary>
	/// <exception cref="Exception">
	/// When no monitors are found, or there is no primary monitor.
	/// </exception>
	public MonitorManager(IConfigContext configContext)
	{
		_configContext = configContext;

		// Initialize the monitors.
		Screen[] screens = Screen.AllScreens;

		_monitors = new Monitor[screens.Length];
		for (int i = 0; i < screens.Length; i++)
		{
			Screen screen = screens[i];
			if (screen.Primary)
			{
				FocusedMonitor = new Monitor(screen);
			}

			_monitors[i] = new Monitor(screen);
		}

		if (_monitors.Length == 0)
		{
			throw new Exception("No monitors were found");
		}
		if (FocusedMonitor == null)
		{
			throw new Exception("Failed to find primary monitor");
		}
	}

	public IMonitor GetMonitorAtPoint(int x, int y)
	{
		Logger.Debug($"Getting monitor at point ({x}, {y})");
		Screen screen = Screen.FromPoint(new System.Drawing.Point(x, y));
		return _monitors.FirstOrDefault(m => m.Name == screen.DeviceName) ?? _monitors[0];
	}
}
