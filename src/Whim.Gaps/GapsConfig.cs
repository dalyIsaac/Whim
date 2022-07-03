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
	public int InnerGap { get; set; }

	/// <summary>
	/// Create a new instance of <see cref="GapsConfig"/>.
	/// </summary>
	/// <param name="outerGap"></param>
	/// <param name="innerGap"></param>
	public GapsConfig(int outerGap = 0, int innerGap = 0)
	{
		OuterGap = outerGap;
		InnerGap = innerGap;
	}
}
