namespace Whim.CommandPalette;

/// <summary>
/// Represents an option in a <see cref="SelectVariantConfig"/> select.
/// </summary>
public interface ISelectOption
{
	/// <summary>
	/// The text to display for this option.
	/// </summary>
	string Text { get; }

	/// <summary>
	/// Whether this option is selected.
	/// </summary>
	bool IsSelected { get; }

	/// <summary>
	/// Whether this option is disabled.
	/// </summary>
	bool IsDisabled { get; }
}
