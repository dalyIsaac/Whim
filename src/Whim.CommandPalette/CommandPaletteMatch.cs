namespace Whim.CommandPalette;

public class CommandPaletteMatch
{
	/// <summary>
	/// A markdown-formatted string representing the match.
	/// </summary>
	public string MatchText { get; }
	public ICommand Command { get; }
	public IKeybind? Keybind { get; }

	public CommandPaletteMatch(string matchText, ICommand command, IKeybind? keybind)
	{
		MatchText = matchText;
		Command = command;
		Keybind = keybind;
	}
}
