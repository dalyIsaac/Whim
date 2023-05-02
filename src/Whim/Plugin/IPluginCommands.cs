using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The commands and keybinds for a plugin.
/// </summary>
public interface IPluginCommands
{
	/// <summary>
	/// The name of the plugin.
	/// </summary>
	string PluginName { get; }

	/// <summary>
	/// The commands for this plugin.
	/// </summary>
	IEnumerable<ICommand> Commands { get; }

	/// <summary>
	/// The keybinds for this plugin.
	/// </summary>
	IEnumerable<(string commandId, IKeybind keybind)> Keybinds { get; }
}
