namespace Whim.CommandPalette;

/// <summary>
/// Config for activating the command palette with a menu.
/// </summary>
public record CommandPaletteMenuActivationConfig : BaseCommandPaletteActivationConfig
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public IMatcher<CommandItem> Matcher { get; init; } = new MostRecentlyUsedMatcher<CommandItem>();
}
