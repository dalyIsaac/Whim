using System.Collections.Generic;

namespace Whim.Bar;

/// <summary>
/// A proxy layout engine to reserve space for the bar in each monitor.
/// </summary>
public class BarLayoutEngine : BaseProxyLayoutEngine
{
	private readonly BarConfig _barConfig;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="BarLayoutEngine"/>.
	/// </summary>
	/// <param name="barConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public BarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine) : base(innerLayoutEngine)
	{
		_barConfig = barConfig;
	}

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location)
	{
		int height = _barConfig.Height + (int)(_barConfig.Margin.Bottom + _barConfig.Margin.Top);
		Location proxiedLocation = new(
			x: location.X,
			y: location.Y + height,
			width: location.Width,
			height: location.Height - height
		);
		return InnerLayoutEngine.DoLayout(proxiedLocation);
	}
}
