using System.Collections.Generic;

namespace Whim.Bar;

/// <summary>
/// A proxy layout engine to reserve space for the bar in each monitor.
/// </summary>
public class ImmutableBarLayoutEngine : BaseProxyLayoutEngine
{
	private readonly BarConfig _barConfig;

	/// <summary>
	/// Creates a new instance of the proxy layout engine <see cref="ImmutableBarLayoutEngine"/>.
	/// </summary>
	/// <param name="barConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public ImmutableBarLayoutEngine(BarConfig barConfig, ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_barConfig = barConfig;
	}

	/// <inheritdoc />
	protected override ILayoutEngine Update(ILayoutEngine newLayoutEngine) =>
		newLayoutEngine == InnerLayoutEngine ? this : new ImmutableBarLayoutEngine(_barConfig, newLayoutEngine);

	/// <inheritdoc />
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor)
	{
		double scale = monitor.ScaleFactor / 100.0;
		int height = (int)(_barConfig.Height * scale);

		Location<int> proxiedLocation =
			new()
			{
				X = location.X,
				Y = location.Y + height,
				Width = location.Width,
				Height = location.Height - height
			};
		return InnerLayoutEngine.DoLayout(proxiedLocation, monitor);
	}
}
