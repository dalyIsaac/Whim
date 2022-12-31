namespace Whim.CommandPalette;

/// <summary>
/// Base config for activating the command palette.
/// </summary>
public record BaseCommandPaletteActivationConfig
{
	/// <summary>
	/// Text hint to show in the command palette.
	/// </summary>
	public string? Hint { get; init; }

	/// <summary>
	/// The text to pre-fill the command palette with.
	/// </summary>
	public string? InitialText { get; init; }

	/// <summary>
	/// Creates a new <see cref="BaseCommandPaletteActivationConfig"/>.
	/// </summary>
	/// <param name="hint"></param>
	/// <param name="initialText"></param>
	public BaseCommandPaletteActivationConfig(string? hint = null, string? initialText = null)
	{
		Hint = hint;
		InitialText = initialText;
	}
}
