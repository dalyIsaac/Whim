namespace Whim.Gaps;

/// <summary>
/// GapsPlugin is a plugin to add gaps between windows in the layout.
/// </summary>
public interface IGapsPlugin : IPlugin
{

	/// <summary>
	/// The configuration for the gaps plugin.
	/// </summary>
	public GapsConfig GapsConfig { get; }

	/// <summary>
	/// Update the gap between the parent layout engine and the area where windows are placed by
	/// the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateOuterGap(int delta);

	/// <summary>
	/// Update the gap between windows by the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateInnerGap(int delta);
}
