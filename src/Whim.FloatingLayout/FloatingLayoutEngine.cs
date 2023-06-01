using System.Collections.Generic;

namespace Whim.FloatingLayout;

/// <inheritdoc />
public class FloatingLayoutEngine : BaseProxyLayoutEngine, IFloatingLayoutEngine
{
	private readonly IContext _context;
	private readonly Dictionary<IWindow, ILocation<double>> _floatingWindowLocations = new();

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="FloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="innerLayoutEngine"></param>
	public FloatingLayoutEngine(IContext context, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_context = context;
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		// Iterate over all windows in _windowToLocation.
		foreach ((IWindow window, ILocation<double> loc) in _floatingWindowLocations)
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
		if (_floatingWindowLocations.ContainsKey(window))
		{
			UpdateFloatingWindow(window);
			return;
		}

		// Pass to the inner layout engine.
		InnerLayoutEngine.AddWindowAtPoint(window, point);
	}

	/// <inheritdoc />
	public void MarkWindowAsFloating(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Marking window {window} as floating");

		if (window != null)
		{
			InnerLayoutEngine.Remove(window);
			UpdateFloatingWindow(window);
		}
	}

	/// <inheritdoc />
	public void MarkWindowAsDocked(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Marking window {window} as docked");

		if (window == null)
		{
			return;
		}

		if (!_floatingWindowLocations.TryGetValue(window, out ILocation<double>? location))
		{
			Logger.Error($"Could not obtain location for floating window {window}");
			return;
		}

		InnerLayoutEngine.AddWindowAtPoint(window, location);
		_floatingWindowLocations.Remove(window);
	}

	/// <inheritdoc />
	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Toggling window {window} floating");

		if (window == null)
		{
			return;
		}

		if (_floatingWindowLocations.ContainsKey(window))
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

		_floatingWindowLocations[window] = location;
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
