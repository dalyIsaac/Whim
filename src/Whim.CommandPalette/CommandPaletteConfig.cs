namespace Whim.CommandPalette;

/// <summary>
/// The configuration for the command palette plugin.
/// </summary>
public class CommandPaletteConfig
{
	/// <summary>
	/// The title of the command palette window.
	/// </summary>
	internal const string Title = "Whim Command Palette";

	/// <summary>
	/// The maximum height of the command palette window, as a percentage of the monitor height.
	/// </summary>
	public int MaxHeightPercent { get; set; } = 40;

	/// <summary>
	/// The maximum width of the command palette window, in pixels.
	/// </summary>
	public int MaxWidthPixels { get; set; } = 800;

	/// <summary>
	/// The top of the command palette window, as a percentage of the monitor height.
	/// </summary>
	public int YPositionPercent { get; set; } = 25;

	/// <summary>
	/// The configuration for command palette activation.
	/// </summary>
	public BaseCommandPaletteActivationConfig ActivationConfig { get; set; } = new CommandPaletteMenuActivationConfig();
}
