using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// Config for activating the command palette with a menu.
/// </summary>
public record MenuVariantConfig : BaseVariantConfig
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public IMatcher<CommandItem> Matcher { get; init; } = new MostRecentlyUsedMatcher<CommandItem>();

	/// <summary>
	/// The commands for this menu.
	/// </summary>
	public required IEnumerable<CommandItem> Commands { get; init; }
}
