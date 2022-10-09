namespace Whim.CommandPalette;

/// <summary>
/// Base config for activating the command palette.
/// </summary>
public class BaseCommandPaletteActivationConfig
{
	/// <summary>
	/// Text hint to show in the command palette.
	/// </summary>
	public string? Hint { get; }

	/// <summary>
	/// The text to pre-fill the command palette with.
	/// </summary>
	public string? InitialText { get; }

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

/// <summary>
/// Config for activating the command palette with a menu.
/// </summary>
public class CommandPaletteMenuActivationConfig : BaseCommandPaletteActivationConfig
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public ICommandPaletteMatcher Matcher { get; }

	/// <summary>
	/// Creates a new <see cref="CommandPaletteMenuActivationConfig"/>.
	/// </summary>
	/// <param name="matcher"></param>
	/// <param name="hint"></param>
	/// <param name="initialText"></param>
	public CommandPaletteMenuActivationConfig(ICommandPaletteMatcher? matcher = null, string? hint = null, string? initialText = null)
		: base(hint, initialText)
	{
		Matcher = matcher ?? new MostRecentlyUsedMatcher();
	}
}

/// <summary>
/// Callback for when the user has pressed enter key in the command palette, and is in free text mode.
/// </summary>
/// <param name="text"></param>
public delegate void CommandPaletteFreeTextCallback(string text);

/// <summary>
/// Config for activating the command palette with free text.
/// </summary>
public class CommandPaletteFreeTextActivationConfig : BaseCommandPaletteActivationConfig
{
	/// <summary>
	/// The callback to invoke when the user has pressed enter.
	/// </summary>
	public CommandPaletteFreeTextCallback Callback { get; }

	/// <summary>
	/// Creates a new <see cref="CommandPaletteFreeTextActivationConfig"/>.
	/// </summary>
	/// <param name="callback"></param>
	/// <param name="hint"></param>
	/// <param name="initialText"></param>
	public CommandPaletteFreeTextActivationConfig(CommandPaletteFreeTextCallback callback, string? hint = null, string? initialText = null)
		: base(hint, initialText)
	{
		Callback = callback;
	}
}
