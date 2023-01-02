namespace Whim.CommandPalette;

/// <summary>
/// Callback for when the user has pressed enter key in the command palette, and is in free text mode.
/// </summary>
/// <param name="text"></param>
public delegate void CommandPaletteFreeTextCallback(string text);

/// <summary>
/// Config for activating the command palette with free text.
/// </summary>
public record CommandPaletteFreeTextActivationConfig : BaseCommandPaletteActivationConfig
{
	/// <summary>
	/// The callback to invoke when the user has pressed enter.
	/// </summary>
	public required CommandPaletteFreeTextCallback Callback { get; init; }
}
