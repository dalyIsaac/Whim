using System.Collections.Generic;

namespace Whim.Gaps;

public class GapsLayoutEngine : BaseProxyLayoutEngine
{
	private readonly GapsConfig _gapsConfig;

	public GapsLayoutEngine(GapsConfig gapsConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_gapsConfig = gapsConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation location)
	{
		Location proxiedLocation = new(
			x: location.X + _gapsConfig.OuterGap,
			y: location.Y + _gapsConfig.OuterGap,
			width: location.Width - _gapsConfig.OuterGap * 2,
			height: location.Height - _gapsConfig.OuterGap * 2
		);
		return _innerLayoutEngine.DoLayout(proxiedLocation);
	}
}
