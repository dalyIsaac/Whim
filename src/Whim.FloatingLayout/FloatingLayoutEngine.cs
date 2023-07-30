using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
internal class FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly IInternalFloatingLayoutPlugin _plugin;
	private readonly ImmutableDictionary<IWindow, ILocation<double>> _floatingWindowLocations;

	/// <inheritdoc />
	public override int Count => InnerLayoutEngine.Count + _floatingWindowLocations.Count;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="FloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="innerLayoutEngine"></param>
	public FloatingLayoutEngine(IContext context, IInternalFloatingLayoutPlugin plugin, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_context = context;
		_plugin = plugin;
		_floatingWindowLocations = ImmutableDictionary<IWindow, ILocation<double>>.Empty;
	}

	private FloatingLayoutEngine(FloatingLayoutEngine oldEngine, ILayoutEngine newInnerLayoutEngine)
		: base(newInnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowLocations = oldEngine._floatingWindowLocations;
	}

	private FloatingLayoutEngine(
		FloatingLayoutEngine oldEngine,
		ILayoutEngine newInnerLayoutEngine,
		ImmutableDictionary<IWindow, ILocation<double>> floatingWindowLocations
	)
		: this(oldEngine, newInnerLayoutEngine)
	{
		_floatingWindowLocations = floatingWindowLocations;
	}

	private FloatingLayoutEngine UpdateInner(ILayoutEngine newInnerLayoutEngine) =>
		InnerLayoutEngine == newInnerLayoutEngine ? this : new FloatingLayoutEngine(this, newInnerLayoutEngine);

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window)
	{
		// If the window is already tracked by this layout engine, or is a new floating window,
		// update the location and return.
		if (IsWindowFloating(window))
		{
			(FloatingLayoutEngine newEngine, bool error) = UpdateWindowLocation(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.AddWindow(window));
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

		return UpdateInner(InnerLayoutEngine.RemoveWindow(window));
	}

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		// If the window is floating, update the location and return.
		if (IsWindowFloating(window))
		{
			(FloatingLayoutEngine newEngine, bool error) = UpdateWindowLocation(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.MoveWindowToPoint(window, point));
	}

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowEdgesInDirection(Direction edge, IPoint<double> deltas, IWindow window)
	{
		// If the window is floating, update the location and return.
		if (IsWindowFloating(window))
		{
			(FloatingLayoutEngine newEngine, bool error) = UpdateWindowLocation(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.MoveWindowEdgesInDirection(edge, deltas, window));
	}

	private bool IsWindowFloating(IWindow window) =>
		_plugin.FloatingWindows.TryGetValue(window, out ISet<LayoutEngineIdentity>? layoutEngines)
		&& layoutEngines.Contains(InnerLayoutEngine.Identity);

	private (FloatingLayoutEngine, bool error) UpdateWindowLocation(IWindow window)
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
			return (this, true);
		}

		IMonitor newMonitor = _context.MonitorManager.GetMonitorAtPoint(newActualLocation);
		ILocation<double> newUnitSquareLocation = newMonitor.WorkingArea.ToUnitSquare(newActualLocation);
		if (newUnitSquareLocation.Equals(oldLocation))
		{
			Logger.Debug($"Location for window {window} has not changed");
			return (this, false);
		}

		ILayoutEngine innerLayoutEngine = InnerLayoutEngine.RemoveWindow(window);
		return (
			new FloatingLayoutEngine(
				this,
				innerLayoutEngine,
				_floatingWindowLocations.SetItem(window, newUnitSquareLocation)
			),
			false
		);
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

	/// <inheritdoc />
	public override IWindow? GetFirstWindow()
	{
		if (InnerLayoutEngine.GetFirstWindow() is IWindow window)
		{
			return window;
		}

		if (_floatingWindowLocations.Count > 0)
		{
			return _floatingWindowLocations.Keys.First();
		}

		return null;
	}

	/// <inheritdoc />
	public override void FocusWindowInDirection(Direction direction, IWindow window)
	{
		if (IsWindowFloating(window))
		{
			// At this stage, we don't have a way to get the window in a child layout engine at
			// a given point.
			// As a workaround, we just focus the first window.
			InnerLayoutEngine.GetFirstWindow()?.Focus();
			return;
		}

		InnerLayoutEngine.FocusWindowInDirection(direction, window);
	}

	/// <inheritdoc />
	public override ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		if (IsWindowFloating(window))
		{
			// At this stage, we don't have a way to get the window in a child layout engine at
			// a given point.
			// For now, we do nothing.
			return this;
		}

		return InnerLayoutEngine.SwapWindowInDirection(direction, window);
	}

	/// <inheritdoc />
	public override bool ContainsWindow(IWindow window) =>
		_floatingWindowLocations.ContainsKey(window) || InnerLayoutEngine.ContainsWindow(window);

	/// <inheritdoc />
	public override void HidePhantomWindows() => InnerLayoutEngine.HidePhantomWindows();
}
