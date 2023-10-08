using System;
using System.Collections.Generic;

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
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		double scaleFactor = monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;

		int outerGap = (int)(_gapsConfig.OuterGap * scale);
		int innerGap = (int)(_gapsConfig.InnerGap * scale);

		int doubleOuterGap = outerGap * 2;
		int doubleInnerGap = innerGap * 2;

		Location<int> proxiedLocation =
			new()
			{
				X = location.X + outerGap,
				Y = location.Y + outerGap,
				Width = location.Width - doubleOuterGap,
				Height = location.Height - doubleOuterGap
			};

		foreach (IWindowState loc in InnerLayoutEngine.DoLayout(proxiedLocation, monitor))
		{
			int x = loc.Location.X + innerGap;
			int y = loc.Location.Y + innerGap;
			int width = loc.Location.Width - doubleInnerGap;
			int height = loc.Location.Height - doubleInnerGap;

			// Get the location of the window with the gaps applied. If the window is too small in
			// a given dimension, then we don't apply the gap in that dimension.
			if (width <= 0)
			{
				x = loc.Location.X;
				width = Math.Max(0, loc.Location.Width);
			}
			if (height <= 0)
			{
				y = loc.Location.Y;
				height = Math.Max(0, loc.Location.Height);
			}

			yield return new WindowState()
			{
				Window = loc.Window,
				Location = new Location<int>()
				{
					X = x,
					Y = y,
					Width = width,
					Height = height
				},
				WindowSize = loc.WindowSize
			};
		}
	}

	/// <inheritdoc />
	public override void FocusWindowInDirection(Direction direction, IWindow window) =>
		InnerLayoutEngine.FocusWindowInDirection(direction, window);

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
}
