using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.FloatingWindow;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating within another layout.
/// </summary>
internal record ProxyFloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly IInternalFloatingWindowPlugin _plugin;
	private readonly ImmutableDictionary<IWindow, IRectangle<double>> _floatingWindowRects;

	/// <inheritdoc />
	public override int Count => InnerLayoutEngine.Count + _floatingWindowRects.Count;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="ProxyFloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="innerLayoutEngine"></param>
	public ProxyFloatingLayoutEngine(
		IContext context,
		IInternalFloatingWindowPlugin plugin,
		ILayoutEngine innerLayoutEngine
	)
		: base(innerLayoutEngine)
	{
		_context = context;
		_plugin = plugin;
		_floatingWindowRects = ImmutableDictionary<IWindow, IRectangle<double>>.Empty;
	}

	private ProxyFloatingLayoutEngine(ProxyFloatingLayoutEngine oldEngine, ILayoutEngine newInnerLayoutEngine)
		: base(newInnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		_floatingWindowRects = oldEngine._floatingWindowRects;
	}

	private ProxyFloatingLayoutEngine(
		ProxyFloatingLayoutEngine oldEngine,
		ILayoutEngine newInnerLayoutEngine,
		ImmutableDictionary<IWindow, IRectangle<double>> floatingWindowRects
	)
		: this(oldEngine, newInnerLayoutEngine)
	{
		_floatingWindowRects = floatingWindowRects;
	}

	/// <summary>
	/// Returns a new instance of <see cref="ProxyFloatingLayoutEngine"/> with the given inner layout engine,
	/// if the inner layout engine has changed, or the <paramref name="gcWindow"/> was floating.
	/// </summary>
	/// <param name="newInnerLayoutEngine">The new inner layout engine.</param>
	/// <param name="gcWindow">
	/// The <see cref="IWindow"/> which triggered the update. If a window has triggered an inner
	/// layout engine update, the window is no longer floating (apart from that one case where we
	/// couldn't get the window's rectangle).
	/// </param>
	/// <returns></returns>
	private ProxyFloatingLayoutEngine UpdateInner(ILayoutEngine newInnerLayoutEngine, IWindow? gcWindow)
	{
		ImmutableDictionary<IWindow, IRectangle<double>> newFloatingWindowRects =
			gcWindow != null ? _floatingWindowRects.Remove(gcWindow) : _floatingWindowRects;

		return InnerLayoutEngine == newInnerLayoutEngine && _floatingWindowRects == newFloatingWindowRects
			? this
			: new ProxyFloatingLayoutEngine(this, newInnerLayoutEngine, newFloatingWindowRects);
	}

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window)
	{
		// If the window is already tracked by this layout engine, or is a new floating window,
		// update the rectangle and return.
		if (IsWindowFloating(window))
		{
			(ProxyFloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
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
				return new ProxyFloatingLayoutEngine(this, InnerLayoutEngine, _floatingWindowRects.Remove(window));
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
			(ProxyFloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
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
			(ProxyFloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.MoveWindowEdgesInDirection(edge, deltas, window), window);
	}

	private bool IsWindowFloating(IWindow? window) =>
		window != null
		&& _plugin.FloatingWindows.TryGetValue(window, out ISet<LayoutEngineIdentity>? layoutEngines)
		&& layoutEngines.Contains(InnerLayoutEngine.Identity);

	private (ProxyFloatingLayoutEngine, bool error) UpdateWindowRectangle(IWindow window)
	{
		ImmutableDictionary<IWindow, IRectangle<double>>? newDict = FloatingUtils.UpdateWindowRectangle(
			_context,
			_floatingWindowRects,
			window
		);

		if (newDict == null)
		{
			return (this, true);
		}

		if (newDict == _floatingWindowRects)
		{
			return (this, false);
		}

		ILayoutEngine innerLayoutEngine = InnerLayoutEngine.RemoveWindow(window);
		return (new ProxyFloatingLayoutEngine(this, innerLayoutEngine, newDict), false);
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
				WindowSize = WindowSize.Normal,
			};
		}

		// Iterate over all windows in the inner layout engine.
		foreach (IWindowState windowState in InnerLayoutEngine.DoLayout(rectangle, monitor))
		{
			yield return windowState;
		}
	}

	/// <inheritdoc />
	public override IWindow? GetFirstWindow() =>
		InnerLayoutEngine.GetFirstWindow() ?? _floatingWindowRects.Keys.FirstOrDefault();

	/// <inheritdoc />
	public override ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window)
	{
		if (IsWindowFloating(window))
		{
			// At this stage, we don't have a way to get the window in a child layout engine at
			// a given point.
			// As a workaround, we just focus the first window.
			InnerLayoutEngine.GetFirstWindow()?.Focus();
			return this;
		}

		return UpdateInner(InnerLayoutEngine.FocusWindowInDirection(direction, window), window);
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

		return UpdateInner(InnerLayoutEngine.SwapWindowInDirection(direction, window), window);
	}

	/// <inheritdoc />
	public override bool ContainsWindow(IWindow window) =>
		_floatingWindowRects.ContainsKey(window) || InnerLayoutEngine.ContainsWindow(window);

	/// <inheritdoc />
	public override ILayoutEngine MinimizeWindowStart(IWindow window) =>
		UpdateInner(InnerLayoutEngine.MinimizeWindowStart(window), window);

	/// <inheritdoc />
	public override ILayoutEngine MinimizeWindowEnd(IWindow window) =>
		UpdateInner(InnerLayoutEngine.MinimizeWindowEnd(window), window);

	/// <inheritdoc />
	public override ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action)
	{
		if (IsWindowFloating(action.Window))
		{
			// At this stage, we don't have a way to get the window in a child layout engine at
			// a given point.
			// For now, we do nothing.
			return this;
		}

		return UpdateInner(InnerLayoutEngine.PerformCustomAction(action), action.Window);
	}
}
