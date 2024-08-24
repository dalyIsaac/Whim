using System;
using System.Collections.Generic;

namespace Whim.Bar;

/// <summary>
/// A proxy layout engine to reserve space for the bar in each monitor.
/// </summary>
public record BarLayoutEngine : BaseProxyLayoutEngine
{
	private readonly BarConfig _barConfig;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="BarLayoutEngine"/>.
	/// </summary>
	/// <param name="barConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public BarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_barConfig = barConfig;
	}

	private BarLayoutEngine UpdateInner(ILayoutEngine newInnerLayoutEngine) =>
		InnerLayoutEngine == newInnerLayoutEngine ? this : new BarLayoutEngine(_barConfig, newInnerLayoutEngine);

	/// <inheritdoc />
	public override int Count => InnerLayoutEngine.Count;

	/// <inheritdoc />
	public override ILayoutEngine AddWindow(IWindow window) => UpdateInner(InnerLayoutEngine.AddWindow(window));

	/// <inheritdoc />
	public override bool ContainsWindow(IWindow window) => InnerLayoutEngine.ContainsWindow(window);

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		double scale = monitor.ScaleFactor / 100.0;
		int height = (int)(_barConfig.Height * scale);

		Rectangle<int> proxiedRect =
			new()
			{
				X = rectangle.X,
				Y = rectangle.Y + height,
				Width = rectangle.Width,
				Height = Math.Max(0, rectangle.Height - height),
			};
		return InnerLayoutEngine.DoLayout(proxiedRect, monitor);
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
