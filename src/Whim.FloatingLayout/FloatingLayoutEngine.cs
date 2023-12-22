using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
internal record FloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly IInternalFloatingLayoutPlugin _plugin;
	private readonly ImmutableDictionary<IWindow, IRectangle<double>> _floatingWindowRects;

	/// <inheritdoc />
	public override int Count => InnerLayoutEngine.Count + _floatingWindowRects.Count;

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
		_floatingWindowRects = ImmutableDictionary<IWindow, IRectangle<double>>.Empty;
	}

	private FloatingLayoutEngine(FloatingLayoutEngine oldEngine, ILayoutEngine newInnerLayoutEngine)
		: base(newInnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowRects = oldEngine._floatingWindowRects;
	}

	private FloatingLayoutEngine(
		FloatingLayoutEngine oldEngine,
		ILayoutEngine newInnerLayoutEngine,
		ImmutableDictionary<IWindow, IRectangle<double>> floatingWindowRects
	)
		: this(oldEngine, newInnerLayoutEngine)
	{
		_floatingWindowRects = floatingWindowRects;
	}

	/// <summary>
	/// Returns a new instance of <see cref="FloatingLayoutEngine"/> with the given inner layout engine,
	/// if the inner layout engine has changed, or the <paramref name="gcWindow"/> was floating.
	/// </summary>
	/// <param name="newInnerLayoutEngine">The new inner layout engine.</param>
	/// <param name="gcWindow">
	/// The <see cref="IWindow"/> which triggered the update. If a window has triggered an inner
	/// layout engine update, the window is no longer floating (apart from that one case where we
	/// couldn't get the window's rectangle).
	/// </param>
	/// <returns></returns>
	private FloatingLayoutEngine UpdateInner(ILayoutEngine newInnerLayoutEngine, IWindow? gcWindow)
	{
		ImmutableDictionary<IWindow, IRectangle<double>> newFloatingWindowRects =
			gcWindow != null ? _floatingWindowRects.Remove(gcWindow) : _floatingWindowRects;

		return InnerLayoutEngine == newInnerLayoutEngine && _floatingWindowRects == newFloatingWindowRects
			? this
			: new FloatingLayoutEngine(this, newInnerLayoutEngine, newFloatingWindowRects);
	}

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window)
	{
		// If the window is already tracked by this layout engine, or is a new floating window,
		// update the rectangle and return.
		if (IsWindowFloating(window))
		{
			(FloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.AddWindow(window), window);
	}

	/// <inheritdoc />
	public override ILayoutEngine RemoveWindow(IWindow window)
	{
		bool isFloating = IsWindowFloating(window);

		// If tracked by this layout engine, remove it.
		// Otherwise, pass to the inner layout engine.
		if (_floatingWindowRects.ContainsKey(window))
		{
			_plugin.MarkWindowAsDockedInLayoutEngine(window, InnerLayoutEngine.Identity);

			// If the window was not supposed to be floating, remove it from the inner layout engine.
			if (isFloating)
			{
				return new FloatingLayoutEngine(this, InnerLayoutEngine, _floatingWindowRects.Remove(window));
			}
		}

		return UpdateInner(InnerLayoutEngine.RemoveWindow(window), window);
	}

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		// If the window is floating, update the rectangle and return.
		if (IsWindowFloating(window))
		{
			(FloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.MoveWindowToPoint(window, point), window);
	}

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowEdgesInDirection(Direction edge, IPoint<double> deltas, IWindow window)
	{
		// If the window is floating, update the rectangle and return.
		if (IsWindowFloating(window))
		{
			(FloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.MoveWindowEdgesInDirection(edge, deltas, window), window);
	}

	private bool IsWindowFloating(IWindow window) =>
		_plugin.FloatingWindows.TryGetValue(window, out ISet<LayoutEngineIdentity>? layoutEngines)
		&& layoutEngines.Contains(InnerLayoutEngine.Identity);

	private (FloatingLayoutEngine, bool error) UpdateWindowRectangle(IWindow window)
	{
		// Try get the old rectangle.
		IRectangle<double>? oldRectangle = _floatingWindowRects.TryGetValue(window, out IRectangle<double>? rectangle)
			? rectangle
			: null;

		// Since the window is floating, we update the rectangle, and return.
		IRectangle<int>? newActualRectangle = _context.NativeManager.DwmGetWindowRectangle(window.Handle);
		if (newActualRectangle == null)
		{
			Logger.Error($"Could not obtain rectangle for floating window {window}");
			return (this, true);
		}

		IMonitor newMonitor = _context.MonitorManager.GetMonitorAtPoint(newActualRectangle);
		IRectangle<double> newUnitSquareRectangle = newMonitor.WorkingArea.ToUnitSquare(newActualRectangle);
		if (newUnitSquareRectangle.Equals(oldRectangle))
		{
			Logger.Debug($"Rectangle for window {window} has not changed");
			return (this, false);
		}

		ILayoutEngine innerLayoutEngine = InnerLayoutEngine.RemoveWindow(window);
		return (
			new FloatingLayoutEngine(
				this,
				innerLayoutEngine,
				_floatingWindowRects.SetItem(window, newUnitSquareRectangle)
			),
			false
		);
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		// Iterate over all windows in _floatingWindowRects.
		foreach ((IWindow window, IRectangle<double> loc) in _floatingWindowRects)
		{
			yield return new WindowState()
			{
				Window = window,
				Rectangle = rectangle.ToMonitor(loc),
				WindowSize = WindowSize.Normal
			};
		}

		// Iterate over all windows in the inner layout engine.
		foreach (IWindowState windowState in InnerLayoutEngine.DoLayout(rectangle, monitor))
		{
			yield return windowState;
		}
	}

	/// <inheritdoc />
	public override IWindow? GetFirstWindow()
	{
		if (InnerLayoutEngine.GetFirstWindow() is IWindow window)
		{
			return window;
		}

		if (_floatingWindowRects.Count > 0)
		{
			return _floatingWindowRects.Keys.First();
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
		_floatingWindowRects.ContainsKey(window) || InnerLayoutEngine.ContainsWindow(window);

	/// <inheritdoc />
	public override ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action)
	{
		if (action.Window != null && IsWindowFloating(action.Window))
		{
			// At this stage, we don't have a way to get the window in a child layout engine at
			// a given point.
			// For now, we do nothing.
			return this;
		}

		return InnerLayoutEngine.PerformCustomAction(action);
	}
}
