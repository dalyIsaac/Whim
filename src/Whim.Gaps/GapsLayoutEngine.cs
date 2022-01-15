using System.Collections.Generic;

namespace Whim.Gaps;

public class GapsLayoutEngine : BaseProxyLayoutEngine
{
	private readonly GapsConfig _gapsConfig;

	public GapsLayoutEngine(GapsConfig gapsConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_gapsConfig = gapsConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		int doubleOuterGap = _gapsConfig.OuterGap * 2;
		int doubleInnerGap = _gapsConfig.InnerGap * 2;

		Location proxiedLocation = new(
			x: location.X + _gapsConfig.OuterGap,
			y: location.Y + _gapsConfig.OuterGap,
			width: location.Width - doubleOuterGap,
			height: location.Height - doubleOuterGap
		);

		foreach (IWindowLocation loc in _innerLayoutEngine.DoLayout(proxiedLocation))
		{
			yield return new WindowLocation(
				window: loc.Window,
				location: new Location(
					x: loc.Location.X + _gapsConfig.InnerGap,
					y: loc.Location.Y + _gapsConfig.InnerGap,
					width: loc.Location.Width - doubleInnerGap,
					height: loc.Location.Height - doubleInnerGap
				),
				windowState: loc.WindowState
			);
		}
	}
}
