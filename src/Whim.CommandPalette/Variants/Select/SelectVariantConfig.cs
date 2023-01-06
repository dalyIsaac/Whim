using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// Config for activating the command palette with a select.
/// </summary>
public record SelectVariantConfig : BaseVariantConfig
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public IMatcher<SelectOption> Matcher { get; init; } = new MostRecentlyUsedMatcher<SelectOption>();

	/// <summary>
	/// Whether this select is a multi-select.
	/// </summary>
	public bool IsMultiSelect { get; init; }

	/// <summary>
	/// The options to display in the select.
	/// </summary>
	public required IEnumerable<SelectOption> Options { get; init; }
}
