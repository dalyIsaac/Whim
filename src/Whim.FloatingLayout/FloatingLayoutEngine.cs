using System.Collections.Generic;

namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
public class FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly FloatingLayoutConfig _floatingLayoutConfig;

	private readonly Dictionary<IWindow, ILocation<double>> _windowToLocation = new();

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="FloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="floatingLayoutConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public FloatingLayoutEngine(
		IContext context,
		FloatingLayoutConfig floatingLayoutConfig,
		ILayoutEngine innerLayoutEngine
	) : base(innerLayoutEngine)
	{
		_context = context;
		_floatingLayoutConfig = floatingLayoutConfig;
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		// Iterate over all windows in _windowToLocation.
		foreach ((IWindow window, ILocation<double> loc) in _windowToLocation)
		{
			yield return new WindowState()
			{
				Window = window,
				Location = location.ToMonitor(loc),
				WindowSize = WindowSize.Normal
			};
		}

		// Iterate over all windows in the inner layout engine.
		foreach (IWindowState windowLocation in InnerLayoutEngine.DoLayout(location, monitor))
		{
			yield return windowLocation;
		}
	}

	/// <summary>
	/// This method is called when a window is moved.
	/// </summary>
	public override void AddWindowAtPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Adding window {window} at point {point}");
		if (_floatingLayoutConfig.IsWindowFloating(window))
		{
			UpdateFloatingWindow(window);
			_floatingLayoutConfig.MarkWindowAsFloating(window);
			return;
		}

		// Pass to the inner layout engine.
		InnerLayoutEngine.AddWindowAtPoint(window, point);
	}

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window.
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Marking window {window} as floating");

		if (window != null)
		{
			InnerLayoutEngine.Remove(window);
			UpdateFloatingWindow(window);
			_floatingLayoutConfig.MarkWindowAsFloating(window);
		}
	}

	/// <summary>
	/// Mark the given <paramref name="window"/> as a docked window.
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Marking window {window} as docked");

		if (window != null)
		{
			ILocation<double> location = _windowToLocation[window];
			InnerLayoutEngine.AddWindowAtPoint(window, location);
			_windowToLocation.Remove(window);
			_floatingLayoutConfig.MarkWindowAsDocked(window);
		}
	}

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
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
		ILocation<int>? location = _context.NativeManager.DwmGetWindowLocation(window.Handle);
		if (location == null)
		{
			return null;
		}

		IMonitor monitor = _context.MonitorManager.GetMonitorAtPoint(location);
		return monitor.WorkingArea.ToUnitSquare(location);
	}
}
