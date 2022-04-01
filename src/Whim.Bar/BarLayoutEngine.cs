using System.Collections.Generic;

namespace Whim.Bar;

public class BarLayoutEngine : BaseProxyLayoutEngine
{
	private readonly BarConfig _barConfig;

	public BarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_barConfig = barConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation<int> location)
	{
		int height = _barConfig.Height + (int)(_barConfig.Margin.Bottom + _barConfig.Margin.Top);
		Location proxiedLocation = new(
			x: location.X,
			y: location.Y + height,
			width: location.Width,
			height: location.Height - height
		);
		return _innerLayoutEngine.DoLayout(proxiedLocation);
	}
}
