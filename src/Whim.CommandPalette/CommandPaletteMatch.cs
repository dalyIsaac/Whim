using System.Collections.ObjectModel;

namespace Whim.CommandPalette;

public class CommandPaletteMatch
{
	public ICommand Command { get; }
	public ReadOnlyCollection<string> Keybind { get; }

	public CommandPaletteMatch(ICommand command, IKeybind? keybind)
	{
		Command = command;

		string[] parts = (keybind?.ToString() ?? "").Split(" + ");
		Keybind = new ReadOnlyCollection<string>(parts);
	}
}
