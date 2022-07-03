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
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location)
	{
		int doubleOuterGap = _gapsConfig.OuterGap * 2;
		int doubleInnerGap = _gapsConfig.InnerGap * 2;

		Location proxiedLocation = new(
			x: location.X + _gapsConfig.OuterGap,
			y: location.Y + _gapsConfig.OuterGap,
			width: location.Width - doubleOuterGap,
			height: location.Height - doubleOuterGap
		);

		foreach (IWindowState loc in InnerLayoutEngine.DoLayout(proxiedLocation))
		{
			yield return new WindowState(
				window: loc.Window,
				location: new Location(
					x: loc.Location.X + _gapsConfig.InnerGap,
					y: loc.Location.Y + _gapsConfig.InnerGap,
					width: loc.Location.Width - doubleInnerGap,
					height: loc.Location.Height - doubleInnerGap
				),
				windowSize: loc.WindowSize
			);
		}
	}
}
