using System.Collections.Generic;

namespace Whim.Bar;

public class BarLayoutEngine : BaseProxyLayoutEngine
{
	private readonly BarConfig _barConfig;

	public BarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_barConfig = barConfig;
	}

	public override IEnumerable<IWindowLocation> DoLayout(ILocation location)
	{
		Location proxiedLocation = new(location.X, location.Y + _barConfig.Height, location.Width, location.Height - _barConfig.Height);
		return _innerLayoutEngine.DoLayout(proxiedLocation);
	}
}
