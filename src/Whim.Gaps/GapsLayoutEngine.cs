using System;
using System.Collections.Generic;
using Whim.FloatingLayout;

namespace Whim.Gaps;

/// <summary>
/// A proxy layout engine to add gaps to the layout.
/// </summary>
public record GapsLayoutEngine : BaseProxyLayoutEngine
{
	private readonly GapsConfig _gapsConfig;

	/// <summary>
	/// Create a new instance of the proxy layout engine <see cref="GapsLayoutEngine"/>.
	/// </summary>
	/// <param name="gapsConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public GapsLayoutEngine(GapsConfig gapsConfig, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_gapsConfig = gapsConfig;
	}

	private GapsLayoutEngine(GapsLayoutEngine oldEngine, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_gapsConfig = oldEngine._gapsConfig;
	}

	private GapsLayoutEngine UpdateInner(ILayoutEngine newInnerLayoutEngine) =>
		InnerLayoutEngine == newInnerLayoutEngine ? this : new GapsLayoutEngine(this, newInnerLayoutEngine);

	/// <inheritdoc />
	public override int Count => InnerLayoutEngine.Count;

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window) => UpdateInner(InnerLayoutEngine.AddWindow(window));

	/// <inheritdoc />
	public override bool ContainsWindow(IWindow window) => InnerLayoutEngine.ContainsWindow(window);

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		if (InnerLayoutEngine.GetLayoutEngine<FloatingLayoutEngine>() is not null)
		{
			foreach (IWindowState windowState in InnerLayoutEngine.DoLayout(rectangle, monitor))
			{
				yield return windowState;
			}

			yield break;
		}

		double scaleFactor = monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;

		int outerGap = (int)(_gapsConfig.OuterGap * scale);
		int innerGap = (int)(_gapsConfig.InnerGap * scale);

		int doubleOuterGap = outerGap * 2;
		int doubleInnerGap = innerGap * 2;

		Rectangle<int> proxiedRect =
			new()
			{
				X = rectangle.X + outerGap,
				Y = rectangle.Y + outerGap,
				Width = rectangle.Width - doubleOuterGap,
				Height = rectangle.Height - doubleOuterGap
			};

		foreach (IWindowState windowState in InnerLayoutEngine.DoLayout(proxiedRect, monitor))
		{
			int x = windowState.Rectangle.X + innerGap;
			int y = windowState.Rectangle.Y + innerGap;
			int width = windowState.Rectangle.Width - doubleInnerGap;
			int height = windowState.Rectangle.Height - doubleInnerGap;

			// Get the rectangle of the window with the gaps applied. If the window is too small in
			// a given dimension, then we don't apply the gap in that dimension.
			if (width <= 0)
			{
				x = windowState.Rectangle.X;
				width = Math.Max(0, windowState.Rectangle.Width);
			}
			if (height <= 0)
			{
				y = windowState.Rectangle.Y;
				height = Math.Max(0, windowState.Rectangle.Height);
			}

			yield return new WindowState()
			{
				Window = windowState.Window,
				Rectangle = new Rectangle<int>()
				{
					X = x,
					Y = y,
					Width = width,
					Height = height
				},
				WindowSize = windowState.WindowSize
			};
		}
	}

	/// <inheritdoc />
	public override ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window) =>
		UpdateInner(InnerLayoutEngine.FocusWindowInDirection(direction, window));

	/// <inheritdoc />
	public override IWindow? GetFirstWindow() => InnerLayoutEngine.GetFirstWindow();

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowEdgesInDirection(Direction edge, IPoint<double> deltas, IWindow window) =>
		UpdateInner(InnerLayoutEngine.MoveWindowEdgesInDirection(edge, deltas, window));

	/// <inheritdoc />
	public override ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) =>
		UpdateInner(InnerLayoutEngine.MoveWindowToPoint(window, point));

	/// <inheritdoc />
	public override ILayoutEngine RemoveWindow(IWindow window) => UpdateInner(InnerLayoutEngine.RemoveWindow(window));

	/// <inheritdoc />
	public override ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
		UpdateInner(InnerLayoutEngine.SwapWindowInDirection(direction, window));

	/// <inheritdoc />
	public override ILayoutEngine MinimizeWindowStart(IWindow window) =>
		UpdateInner(InnerLayoutEngine.MinimizeWindowStart(window));

	/// <inheritdoc />
	public override ILayoutEngine MinimizeWindowEnd(IWindow window) =>
		UpdateInner(InnerLayoutEngine.MinimizeWindowEnd(window));

	/// <inheritdoc />
	public override ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) =>
		UpdateInner(InnerLayoutEngine.PerformCustomAction(action));
}
