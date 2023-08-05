using System;
using System.Collections.Generic;

namespace Whim.LayoutPreview;

internal class LayoutPreviewCommands : IPluginCommands
{
	private readonly IContext _context;
	private readonly IPlugin _plugin;

	public LayoutPreviewCommands(IContext context, IPlugin plugin)
	{
		_context = context;
		_plugin = plugin;
	}

	public string PluginName => _plugin.Name;

	public IEnumerable<ICommand> Commands => Array.Empty<ICommand>();

	public IEnumerable<(string commandId, IKeybind keybind)> Keybinds =>
		Array.Empty<(string commandId, IKeybind keybind)>();
}
