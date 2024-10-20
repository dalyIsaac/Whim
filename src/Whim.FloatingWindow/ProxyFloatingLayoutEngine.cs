using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.FloatingWindow;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating within another layout.
/// </summary>
public record ProxyFloatingLayoutEngine : BaseProxyLayoutEngine
{
	private readonly IContext _context;
	private readonly IFloatingWindowPlugin _plugin;

	/// <summary>
	/// The positions of the floating windows.
	/// </summary>
	public ImmutableDictionary<IWindow, IRectangle<double>> FloatingWindowRects { get; }

	/// <summary>
	/// The former positions of the minimized windows.
	/// </summary>
	public ImmutableDictionary<IWindow, IRectangle<double>> MinimizedWindowRects { get; }

	/// <inheritdoc />
	public override int Count => InnerLayoutEngine.Count + FloatingWindowRects.Count;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="ProxyFloatingLayoutEngine"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="plugin"></param>
	/// <param name="innerLayoutEngine"></param>
	public ProxyFloatingLayoutEngine(IContext context, IFloatingWindowPlugin plugin, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_context = context;
		_plugin = plugin;
		FloatingWindowRects = ImmutableDictionary<IWindow, IRectangle<double>>.Empty;
		MinimizedWindowRects = ImmutableDictionary<IWindow, IRectangle<double>>.Empty;
	}

	private ProxyFloatingLayoutEngine(
		ProxyFloatingLayoutEngine oldEngine,
		ILayoutEngine newInnerLayoutEngine,
		ImmutableDictionary<IWindow, IRectangle<double>> floatingWindowRects,
		ImmutableDictionary<IWindow, IRectangle<double>> minimizedWindows
	)
		: base(newInnerLayoutEngine)
	{
		_context = oldEngine._context;
		_plugin = oldEngine._plugin;
		FloatingWindowRects = floatingWindowRects;
		MinimizedWindowRects = minimizedWindows;
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
			gcWindow != null ? FloatingWindowRects.Remove(gcWindow) : FloatingWindowRects;

		ImmutableDictionary<IWindow, IRectangle<double>> newMinimizedWindows =
			gcWindow != null ? MinimizedWindowRects.Remove(gcWindow) : MinimizedWindowRects;

		return
			InnerLayoutEngine == newInnerLayoutEngine
			&& FloatingWindowRects == newFloatingWindowRects
			&& MinimizedWindowRects == newMinimizedWindows
			? this
			: new ProxyFloatingLayoutEngine(this, newInnerLayoutEngine, newFloatingWindowRects, newMinimizedWindows);
	}

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window)
	{
		if (IsWindowFloatingInLayoutEngine(window))
		{
			bool shouldDock = !_plugin.FloatingWindows.Contains(window.Handle);

			if (shouldDock)
			{
				ILayoutEngine newInnerLayoutEngine = InnerLayoutEngine.AddWindow(window);
				return new ProxyFloatingLayoutEngine(
					this,
					newInnerLayoutEngine,
					FloatingWindowRects.Remove(window),
					MinimizedWindowRects.Remove(window)
				);
			}
		}

		if (IsWindowFloating(window))
		{
			// If the window is floating, update the rectangle and return.
			(ProxyFloatingLayoutEngine newEngine, bool error) = UpdateWindowRectangle(window);
			if (!error)
			{
				return newEngine;
			}
		}

		return UpdateInner(InnerLayoutEngine.AddWindow(window), window);
	}

	/// <inheritdoc />
	public override ILayoutEngine RemoveWindow(IWindow window) =>
		UpdateInner(InnerLayoutEngine.RemoveWindow(window), window);

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		if (IsWindowFloatingInLayoutEngine(window))
		{
			bool shouldDock = !_plugin.FloatingWindows.Contains(window.Handle);

			if (shouldDock)
			{
				return UpdateInner(InnerLayoutEngine.MoveWindowToPoint(window, point), window);
			}
		}

		if (IsWindowFloating(window))
		{
			// If the window is floating, update the rectangle and return.
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

	private bool IsWindowFloating(IWindow? window)
	{
		if (window == null)
		{
			return false;
		}

		if (IsWindowFloatingInLayoutEngine(window))
		{
			return true;
		}

		return _plugin.FloatingWindows.Contains(window.Handle);
	}

	private bool IsWindowFloatingInLayoutEngine(IWindow window) =>
		FloatingWindowRects.ContainsKey(window) || MinimizedWindowRects.ContainsKey(window);

	private (ProxyFloatingLayoutEngine, bool error) UpdateWindowRectangle(IWindow window)
	{
		ImmutableDictionary<IWindow, IRectangle<double>>? newDict = FloatingUtils.UpdateWindowRectangle(
			_context,
			FloatingWindowRects,
			window
		);

		if (newDict == null)
		{
			return (this, true);
		}

		if (newDict == FloatingWindowRects)
		{
			return (this, false);
		}

		ILayoutEngine innerLayoutEngine = InnerLayoutEngine.RemoveWindow(window);
		return (new ProxyFloatingLayoutEngine(this, innerLayoutEngine, newDict, MinimizedWindowRects), false);
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		foreach ((IWindow window, IRectangle<double> loc) in FloatingWindowRects)
		{
			yield return new WindowState()
			{
				Window = window,
				Rectangle = rectangle.ToMonitor(loc),
				WindowSize = WindowSize.Normal,
			};
		}

		foreach ((IWindow window, IRectangle<double> loc) in MinimizedWindowRects)
		{
			yield return new WindowState()
			{
				Window = window,
				Rectangle = rectangle.ToMonitor(loc),
				WindowSize = WindowSize.Minimized,
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
		InnerLayoutEngine.GetFirstWindow()
		?? FloatingWindowRects.Keys.FirstOrDefault()
		?? MinimizedWindowRects.Keys.FirstOrDefault();

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
		FloatingWindowRects.ContainsKey(window)
		|| MinimizedWindowRects.ContainsKey(window)
		|| InnerLayoutEngine.ContainsWindow(window);

	/// <inheritdoc />
	public override ILayoutEngine MinimizeWindowStart(IWindow window)
	{
		if (MinimizedWindowRects.ContainsKey(window))
		{
			return this;
		}

		if (FloatingWindowRects.TryGetValue(window, out IRectangle<double>? oldPosition))
		{
			return new ProxyFloatingLayoutEngine(
				this,
				InnerLayoutEngine,
				FloatingWindowRects.Remove(window),
				MinimizedWindowRects.Add(window, oldPosition)
			);
		}

		return UpdateInner(InnerLayoutEngine.MinimizeWindowStart(window), window);
	}

	/// <inheritdoc />
	public override ILayoutEngine MinimizeWindowEnd(IWindow window)
	{
		if (!MinimizedWindowRects.ContainsKey(window))
		{
			return this;
		}

		if (MinimizedWindowRects.TryGetValue(window, out IRectangle<double>? oldPosition))
		{
			return new ProxyFloatingLayoutEngine(
				this,
				InnerLayoutEngine,
				FloatingWindowRects.Add(window, oldPosition),
				MinimizedWindowRects.Remove(window)
			);
		}

		return UpdateInner(InnerLayoutEngine.MinimizeWindowEnd(window), window);
	}

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
