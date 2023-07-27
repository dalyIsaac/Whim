using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
public class FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly FloatingLayoutPlugin _plugin;
	private readonly ImmutableDictionary<IWindow, ILocation<double>> _floatingWindowLocations;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="FloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="innerLayoutEngine"></param>
	public FloatingLayoutEngine(IContext context, FloatingLayoutPlugin plugin, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_context = context;
		_plugin = plugin;
		_floatingWindowLocations = ImmutableDictionary<IWindow, ILocation<double>>.Empty;
	}

	private FloatingLayoutEngine(
		FloatingLayoutEngine oldEngine,
		ILayoutEngine newInnerLayoutEngine,
		ImmutableDictionary<IWindow, ILocation<double>> floatingWindowLocations
	)
		: base(newInnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowLocations = floatingWindowLocations;
	}

	/// <inheritdoc />
	protected override ILayoutEngine Update(ILayoutEngine newInnerLayoutEngine) =>
		newInnerLayoutEngine == InnerLayoutEngine
			? this
			: new FloatingLayoutEngine(_context, _plugin, newInnerLayoutEngine);

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window)
	{
		// If the window is already tracked by this layout engine, or is a new floating window,
		// update the location and return.
		if (IsWindowFloating(window))
		{
			return UpdateWindowLocation(window);
		}

		return base.AddWindow(window);
	}

	/// <inheritdoc />
	public override ILayoutEngine RemoveWindow(IWindow window)
	{
		// If tracked by this layout engine, remove it.
		// Otherwise, pass to the inner layout engine.
		if (_floatingWindowLocations.ContainsKey(window))
		{
			_plugin.FloatingWindows.Remove(window);
			return new FloatingLayoutEngine(this, InnerLayoutEngine, _floatingWindowLocations.Remove(window));
		}

		return base.RemoveWindow(window);
	}

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		// If the window is floating, update the location and return.
		if (IsWindowFloating(window))
		{
			return UpdateWindowLocation(window);
		}

		return base.MoveWindowToPoint(window, point);
	}

	private bool IsWindowFloating(IWindow window) =>
		_plugin.FloatingWindows.TryGetValue(window, out ISet<LayoutEngineIdentity>? layoutEngines)
		&& layoutEngines.Contains(InnerLayoutEngine.Identity);

	private FloatingLayoutEngine UpdateWindowLocation(IWindow window)
	{
		// Try get the old location.
		ILocation<double>? oldLocation = _floatingWindowLocations.TryGetValue(window, out ILocation<double>? location)
			? location
			: null;

		// Since the window is floating, we update the location, and return.
		ILocation<int>? newActualLocation = _context.NativeManager.DwmGetWindowLocation(window.Handle);
		if (newActualLocation == null)
		{
			Logger.Error($"Could not obtain location for floating window {window}");
			return this;
		}

		IMonitor newMonitor = _context.MonitorManager.GetMonitorAtPoint(newActualLocation);
		ILocation<double> newUnitSquareLocation = newMonitor.WorkingArea.ToUnitSquare(newActualLocation);
		if (newUnitSquareLocation.Equals(oldLocation))
		{
			Logger.Debug($"Location for window {window} has not changed");
			return this;
		}

		ILayoutEngine innerLayoutEngine = InnerLayoutEngine.RemoveWindow(window);
		return new FloatingLayoutEngine(
			this,
			innerLayoutEngine,
			_floatingWindowLocations.SetItem(window, newUnitSquareLocation)
		);
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		List<IWindow> windowsToRemove = new();

		// Iterate over all windows in _windowToLocation.
		foreach ((IWindow window, ILocation<double> loc) in _floatingWindowLocations)
		{
			if (!_plugin.FloatingWindows.ContainsKey(window))
			{
				windowsToRemove.Add(window);
				continue;
			}

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

		// Remove all windows that are no longer floating.
		foreach (IWindow window in windowsToRemove)
		{
			_floatingWindowLocations.Remove(window);
		}
	}
}
