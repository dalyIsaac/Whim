namespace Whim.CommandPalette;

/// <summary>
/// Base config for activating the command palette.
/// </summary>
public abstract record BaseVariantConfig
{
	/// <summary>
	/// Text hint to show in the command palette.
	/// </summary>
	public string? Hint { get; init; }

	/// <summary>
	/// The text to pre-fill the command palette with.
	/// </summary>
	public string? InitialText { get; init; }
}
