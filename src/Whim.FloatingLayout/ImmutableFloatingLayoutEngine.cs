using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
public class ImmutableFloatingLayoutEngine : ImmutableBaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly IFloatingLayoutPlugin _plugin;
	private readonly ImmutableDictionary<IWindow, ILocation<double>> _floatingWindowLocations;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="ImmutableFloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="innerLayoutEngine"></param>
	public ImmutableFloatingLayoutEngine(
		IContext context,
		IFloatingLayoutPlugin plugin,
		IImmutableLayoutEngine innerLayoutEngine
	)
		: base(innerLayoutEngine)
	{
		_context = context;
		_plugin = plugin;
		_floatingWindowLocations = ImmutableDictionary<IWindow, ILocation<double>>.Empty;
	}

	private ImmutableFloatingLayoutEngine(
		ImmutableFloatingLayoutEngine oldEngine,
		IImmutableLayoutEngine newInnerLayoutEngine
	)
		: base(newInnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowLocations = oldEngine._floatingWindowLocations;
	}

	private ImmutableFloatingLayoutEngine(
		ImmutableFloatingLayoutEngine oldEngine,
		ImmutableDictionary<IWindow, ILocation<double>> floatingWindowLocations
	)
		: base(oldEngine.InnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowLocations = floatingWindowLocations;
	}

	/// <inheritdoc />
	protected override IImmutableLayoutEngine Update(IImmutableLayoutEngine newInnerLayoutEngine) =>
		newInnerLayoutEngine == InnerLayoutEngine
			? this
			: new ImmutableFloatingLayoutEngine(this, newInnerLayoutEngine);

	/// <inheritdoc />
	public override IImmutableLayoutEngine Add(IWindow window)
	{
		// If the window is already tracked by this layout engine, or is a new floating window,
		// update the location and return.
		if (_floatingWindowLocations.ContainsKey(window) || _plugin.FloatingWindows.Contains(window))
		{
			return UpdateWindowLocation(window, _floatingWindowLocations[window]);
		}

		return base.Add(window);
	}

	private ImmutableFloatingLayoutEngine UpdateWindowLocation(IWindow window, ILocation<double>? oldLocation)
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

		return this;
	}

	/// <inheritdoc />
	public override IImmutableLayoutEngine Remove(IWindow window)
	{
		// If tracked by this layout engine, remove it.
		// Otherwise, pass to the inner layout engine.
		if (_floatingWindowLocations.ContainsKey(window))
		{
			if (_plugin is IInternalFloatingLayoutPlugin internalPlugin)
			{
				internalPlugin.MutableFloatingWindows.Remove(window);
			}

			return new ImmutableFloatingLayoutEngine(this, _floatingWindowLocations.Remove(window));
		}

		return base.Remove(window);
	}

	/// <inheritdoc />
	public override IImmutableLayoutEngine AddAtPoint(IWindow window, IPoint<double> point) => Add(window);

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
