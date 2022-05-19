namespace Whim.CommandPalette;

public class CommandPaletteMatch
{
	public ICommand Command { get; }
	public string? Keybind { get; }

	public CommandPaletteMatch(ICommand command, IKeybind? keybind)
	{
		Command = command;
		Keybind = keybind?.ToString() ?? "";
	}
}
