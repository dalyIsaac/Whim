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
	/// The matcher to use when filtering for commands.
	/// </summary>
	public ICommandPaletteMatcher Matcher { get; set; } = new MostRecentlyUsedMatcher();
}
