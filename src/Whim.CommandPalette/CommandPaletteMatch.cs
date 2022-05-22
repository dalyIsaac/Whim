using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whim.CommandPalette;

public class CommandPaletteMatch
{
	public ICommand Command { get; }
	public HighlightedText Title { get; }
	public ReadOnlyCollection<string> AllKeys { get; }

	public CommandPaletteMatch(ICommand command, IKeybind? keybind)
	{
		Command = command;
		Title = new();

		switch (keybind)
		{
			case not null:
			{
				AllKeys = keybind is Keybind k ? k.AllKeys : new Keybind(keybind.Modifiers, keybind.Key).AllKeys;
				break;
			}

			default:
				AllKeys = new ReadOnlyCollection<string>(new List<string>());
				break;
		}
	}
}
