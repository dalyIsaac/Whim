namespace Whim.CommandPalette;

/// <summary>
/// Represents an option in a <see cref="CommandPaletteSelectActivationConfig"/> select.
/// </summary>
public interface ISelectOption<T>
{
	/// <summary>
	/// The text to display for this option.
	/// </summary>
	string Text { get; }

	/// <summary>
	/// The value of this option.
	/// </summary>
	T Value { get; }

	/// <summary>
	/// Whether this option is selected.
	/// </summary>
	bool IsSelected { get; }

	/// <summary>
	/// Whether this option is disabled.
	/// </summary>
	bool IsDisabled { get; }
}
