using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// Callback for when the user has selected from the available options.
/// All items are passed in, so that the callback can determine which items were selected.
/// </summary>
/// <param name="allItems"></param>
public delegate void SelectVariantCallback(IEnumerable<SelectOption> allItems);

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

	/// <summary>
	/// The callback to invoke when the user has selected from the available options.
	/// All items are passed in, so that the callback can determine which items were selected.
	/// </summary>
	public required SelectVariantCallback Callback { get; init; }
}
