using System.Collections.Generic;

namespace Whim.NewBar;

public class BarLayoutEngine : BaseProxyLayoutEngine
{
	private readonly BarConfig _barConfig;

	public BarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_barConfig = barConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		int height = _barConfig.Height + _barConfig.Margin;
		Location proxiedLocation = new(
			x: location.X,
			y: location.Y + height,
			width: location.Width,
			height: location.Height - height
		);
		return _innerLayoutEngine.DoLayout(proxiedLocation);
	}
}
