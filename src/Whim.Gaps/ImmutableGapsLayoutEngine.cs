using System.Collections.Generic;

namespace Whim.Gaps;

/// <summary>
/// A proxy layout engine to add gaps to the layout.
/// </summary>
public class ImmutableGapsLayoutEngine : ImmutableBaseProxyLayoutEngine
{
	private readonly GapsConfig _gapsConfig;

	/// <summary>
	/// Create a new instance of the proxy layout engine <see cref="ImmutableGapsLayoutEngine"/>.
	/// </summary>
	/// <param name="gapsConfig"></param>
	/// <param name="innerLayoutEngine"></param>
	public ImmutableGapsLayoutEngine(GapsConfig gapsConfig, IImmutableLayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine)
	{
		_gapsConfig = gapsConfig;
	}

	/// <inheritdoc />
	protected override IImmutableLayoutEngine Update(IImmutableLayoutEngine newLayoutEngine) =>
		newLayoutEngine == InnerLayoutEngine ? this : new ImmutableGapsLayoutEngine(_gapsConfig, newLayoutEngine);

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
			yield return new WindowState()
			{
				Window = loc.Window,
				Location = new Location<int>()
				{
					X = loc.Location.X + innerGap,
					Y = loc.Location.Y + innerGap,
					Width = loc.Location.Width - doubleInnerGap,
					Height = loc.Location.Height - doubleInnerGap
				},
				WindowSize = loc.WindowSize
			};
		}
	}
}
