namespace Whim.Gaps;

public class GapsConfig
{
	/// <summary>
	/// The gap between the parent layout engine and the area where windows are placed.
	/// </summary>
	public int OuterGap { get; set; } = 0;

	/// <summary>
	/// The gap between windows.
	/// </summary>
	public int InnerGap { get; set; } = 0;

	public GapsConfig(int outerGap = 0, int innerGap = 0)
	{
		OuterGap = outerGap;
		InnerGap = innerGap;
	}
}
