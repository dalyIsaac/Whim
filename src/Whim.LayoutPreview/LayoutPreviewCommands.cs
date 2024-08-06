using System.Collections.Generic;

namespace Whim.LayoutPreview;

internal class LayoutPreviewCommands(IPlugin plugin) : IPluginCommands
{
	private readonly IPlugin _plugin = plugin;

	public string PluginName => _plugin.Name;

	public IEnumerable<ICommand> Commands => [];

	public IEnumerable<(string commandId, IKeybind keybind)> Keybinds => [];
}
