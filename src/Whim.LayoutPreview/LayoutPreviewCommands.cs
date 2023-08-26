using System;
using System.Collections.Generic;

namespace Whim.LayoutPreview;

internal class LayoutPreviewCommands : IPluginCommands
{
	private readonly IPlugin _plugin;

	public LayoutPreviewCommands(IPlugin plugin)
	{
		_plugin = plugin;
	}

	public string PluginName => _plugin.Name;

	public IEnumerable<ICommand> Commands => Array.Empty<ICommand>();

	public IEnumerable<(string commandId, IKeybind keybind)> Keybinds =>
		Array.Empty<(string commandId, IKeybind keybind)>();
}
