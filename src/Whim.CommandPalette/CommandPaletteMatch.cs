namespace Whim.CommandPalette;

public class CommandPaletteMatch
{
	/// <summary>
	/// A markdown-formatted string representing the match.
	/// </summary>
	public string Text { get; }
	public ICommand Command { get; }
	public IKeybind? Keybind { get; }

	public CommandPaletteMatch(string text, ICommand command, IKeybind? keybind)
	{
		Text = text;
		Command = command;
		Keybind = keybind;
	}
}
