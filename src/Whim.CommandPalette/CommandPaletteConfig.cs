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
	/// The configuration for command palette activation.
	/// </summary>
	public BaseCommandPaletteActivationConfig ActivationConfig { get; set; } = new CommandPaletteMenuActivationConfig();
}
