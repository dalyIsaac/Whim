using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
public class FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly IFloatingLayoutPlugin _plugin;
	private readonly ImmutableDictionary<IWindow, ILocation<double>> _floatingWindowLocations;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="FloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="innerLayoutEngine"></param>
	public FloatingLayoutEngine(IContext context, IFloatingLayoutPlugin plugin, ILayoutEngine innerLayoutEngine)
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
		ImmutableDictionary<IWindow, ILocation<double>> floatingWindowLocations
	)
		: base(oldEngine.InnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowLocations = floatingWindowLocations;
	}

	/// <inheritdoc />
	protected override ILayoutEngine Update(ILayoutEngine newInnerLayoutEngine) =>
		newInnerLayoutEngine == InnerLayoutEngine ? this : new FloatingLayoutEngine(this, newInnerLayoutEngine);

	/// <inheritdoc />
	public override ILayoutEngine Add(IWindow window)
	{
		// If the window is already tracked by this layout engine, or is a new floating window,
		// update the location and return.
		if (
			_floatingWindowLocations.TryGetValue(window, out ILocation<double>? location)
			|| _plugin.FloatingWindows.Contains(window)
		)
		{
			return UpdateWindowLocation(window, location);
		}

		return base.Add(window);
	}

	private FloatingLayoutEngine UpdateWindowLocation(IWindow window, ILocation<double>? oldLocation)
	{
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

		return new FloatingLayoutEngine(this, _floatingWindowLocations.SetItem(window, newUnitSquareLocation));
	}

	/// <inheritdoc />
	public override ILayoutEngine Remove(IWindow window)
	{
		// If tracked by this layout engine, remove it.
		// Otherwise, pass to the inner layout engine.
		if (_floatingWindowLocations.ContainsKey(window))
		{
			if (_plugin is IInternalFloatingLayoutPlugin internalPlugin)
			{
				internalPlugin.MutableFloatingWindows.Remove(window);
			}

			return new FloatingLayoutEngine(this, _floatingWindowLocations.Remove(window));
		}

		return base.Remove(window);
	}

	/// <inheritdoc />
	public override ILayoutEngine AddAtPoint(IWindow window, IPoint<double> point) => Add(window);

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
}
