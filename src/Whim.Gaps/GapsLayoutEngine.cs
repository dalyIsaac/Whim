using System.Collections.Generic;

namespace Whim.Gaps;

/// <summary>
/// A proxy layout engine to add gaps to the layout.
/// </summary>
public class GapsLayoutEngine : BaseProxyLayoutEngine
{
	private readonly GapsConfig _gapsConfig;

	/// <summary>
	/// Create a new instance of the proxy layout engine <see cref="GapsLayoutEngine"/>.
	/// </summary>
	/// <param name="gapsConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public GapsLayoutEngine(GapsConfig gapsConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_gapsConfig = gapsConfig;
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		double scaleFactor = monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;

		int outerGap = (int)(_gapsConfig.OuterGap * scale);
		int innerGap = (int)(_gapsConfig.InnerGap * scale);

		int doubleOuterGap = outerGap * 2;
		int doubleInnerGap = innerGap * 2;

		Location proxiedLocation =
			new(
				x: location.X + outerGap,
				y: location.Y + outerGap,
				width: location.Width - doubleOuterGap,
				height: location.Height - doubleOuterGap
			);

		foreach (IWindowState loc in InnerLayoutEngine.DoLayout(proxiedLocation, monitor))
		{
			yield return new WindowState(
				window: loc.Window,
				location: new Location(
					x: loc.Location.X + innerGap,
					y: loc.Location.Y + innerGap,
					width: loc.Location.Width - doubleInnerGap,
					height: loc.Location.Height - doubleInnerGap
				),
				windowSize: loc.WindowSize
			);
		}
	}
}
