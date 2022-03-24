using System.Collections.Generic;

namespace Whim.FloatingLayout;

public class FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IConfigContext _configContext;
	private readonly FloatingLayoutConfig _floatingLayoutConfig;

	private readonly Dictionary<IWindow, ILocation<double>> _windowToLocation = new();

	public FloatingLayoutEngine(IConfigContext configContext, FloatingLayoutConfig floatingLayoutConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_configContext = configContext;
		_floatingLayoutConfig = floatingLayoutConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		// Iterate over all windows in _windowToLocation.
		foreach ((IWindow window, ILocation<double> loc) in _windowToLocation)
		{
			yield return new WindowLocation(window, location.ToMonitor(loc), WindowState.Normal);
		}

		// Iterate over all windows in the inner layout engine.
		foreach (IWindowLocation windowLocation in _innerLayoutEngine.DoLayout(location))
		{
			yield return windowLocation;
		}
	}

	/// <summary>
	/// This method is called when a window is moved.
	/// </summary>
	public override void AddWindowAtPoint(IWindow window, IPoint<double> point, bool isPhantom = false)
	{
		Logger.Debug($"Adding window {window} at point {point}. isPhantom={isPhantom}");
		if (_floatingLayoutConfig.IsWindowFloating(window))
		{
			UpdateFloatingWindow(window);
			_floatingLayoutConfig.MarkWindowAsFloating(window);
			return;
		}

		// Pass to the inner layout engine.
		_innerLayoutEngine.AddWindowAtPoint(window, point);
	}

	public void MarkWindowAsFloating(IWindow? window = null)
	{
		window ??= _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Marking window {window} as floating");

		if (window != null)
		{
			_innerLayoutEngine.Remove(window);
			UpdateFloatingWindow(window);
			_floatingLayoutConfig.MarkWindowAsFloating(window);
		}
	}

	public void MarkWindowAsDocked(IWindow? window = null)
	{
		window ??= _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Marking window {window} as docked");

		if (window != null)
		{
			_innerLayoutEngine.Add(window);
			_windowToLocation.Remove(window);
			_floatingLayoutConfig.MarkWindowAsDocked(window);
		}
	}

	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _configContext.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Toggling window {window} floating");

		if (window == null)
		{
			return;
		}

		if (_floatingLayoutConfig.IsWindowFloating(window))
		{
			MarkWindowAsDocked(window);
		}
		else
		{
			MarkWindowAsFloating(window);
		}
	}

	private void UpdateFloatingWindow(IWindow window)
	{
		// Since the window is floating, we update the location, and return.
		ILocation<double>? location = GetUnitLocation(window);
		if (location == null)
		{
			Logger.Error($"Could not obtain location for floating window {window}");
			return;
		}

		_windowToLocation[window] = location;
	}

	private ILocation<double>? GetUnitLocation(IWindow window)
	{
		ILocation<int>? location = Win32Helper.DwmGetWindowLocation(window.Handle);
		if (location == null)
		{
			return null;
		}

		IMonitor monitor = _configContext.MonitorManager.GetMonitorAtPoint(location);
		return monitor.ToUnitSquare(location);
	}
}
