namespace Whim.Gaps;

/// <summary>
/// Configuration for the gaps plugin.
/// </summary>
public class GapsConfig
{
	/// <summary>
	/// The gap between the parent layout engine and the area where windows are placed.
	/// </summary>
	public int OuterGap { get; set; }

	/// <summary>
	/// The gap between windows.
	/// </summary>
	public int InnerGap { get; set; } = 10;

	/// <summary>
	/// The default delta used by the commands <c>gaps.outer.increase</c> and
	/// <c>gaps.outer.decrease</c>.
	/// </summary>
	public int DefaultOuterDelta { get; set; } = 2;

	/// <summary>
	/// The default delta used by the commands <c>gaps.inner.increase</c> and
	/// <c>gaps.inner.decrease</c>.
	/// </summary>
	public int DefaultInnerDelta { get; set; } = 2;
}
