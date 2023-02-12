namespace Whim.CommandPalette;

/// <summary>
/// Represents an option of a <see cref="SelectVariantConfig"/>
/// </summary>
public record SelectOption
{
	/// <summary>
	/// The unique identifier of this option.
	/// </summary>
	public required string Id { get; set; }

	/// <summary>
	/// The title to display for this option.
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// Whether this option is selected.
	/// </summary>
	public bool IsSelected { get; set; }

	/// <summary>
	/// Whether this option is enabled.
	/// </summary>
	public bool IsEnabled { get; set; }
}
