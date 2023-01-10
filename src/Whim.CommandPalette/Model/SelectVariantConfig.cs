using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// Config for activating the command palette with radio buttons or CheckBox (referred to as
/// "Select").
/// </summary>
public abstract record SelectVariantConfig : BaseVariantConfig
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public IMatcher<SelectOption> Matcher { get; init; } = new MostRecentlyUsedMatcher<SelectOption>();

	/// <summary>
	/// The options to display in the select.
	/// </summary>
	public required IEnumerable<SelectOption> Options { get; init; }
}
