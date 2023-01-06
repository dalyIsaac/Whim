namespace Whim.CommandPalette;

/// <summary>
/// Callback for when the user has pressed enter key in the command palette, and is in free text mode.
/// </summary>
/// <param name="text"></param>
public delegate void FreeTextVariantCallback(string text);

/// <summary>
/// Config for activating the command palette with free text.
/// </summary>
public record FreeTextVariantConfig : BaseVariantConfig
{
	/// <summary>
	/// The callback to invoke when the user has pressed enter.
	/// </summary>
	public required FreeTextVariantCallback Callback { get; init; }

	/// <summary>
	/// The prompt to display in the command palette.
	/// </summary>
	public required string Prompt { get; init; }
}
