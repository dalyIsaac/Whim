using System.Collections.Generic;
using Whim.CommandPalette;

namespace Whim.TreeLayout.CommandPalette;

/// <summary>
/// This plugin contains commands to interact with the tree layout via the command palette.
/// </summary>
public class TreeLayoutCommandPalettePlugin : IPlugin
{
	private readonly ITreeLayoutPlugin _treeLayoutPlugin;
	private readonly ICommandPalettePlugin _commandLayoutPlugin;

	/// <inheritdoc/>
	public string Name => "whim.tree_layout.command_palette";

	/// <summary>
	/// Creates a new instance of the tree layout command palette plugin.
	/// </summary>
	/// <param name="treeLayoutPlugin"></param>
	/// <param name="commandPalettePlugin"></param>
	public TreeLayoutCommandPalettePlugin(
		ITreeLayoutPlugin treeLayoutPlugin,
		ICommandPalettePlugin commandPalettePlugin
	)
	{
		_treeLayoutPlugin = treeLayoutPlugin;
		_commandLayoutPlugin = commandPalettePlugin;
	}

	/// <inheritdoc />
	public IEnumerable<CommandItem> Commands =>
		new TreeLayoutCommandPalettePluginCommands(this, _treeLayoutPlugin, _commandLayoutPlugin);

	/// <inheritdoc/>
	public void PostInitialize() { }

	/// <inheritdoc/>
	public void PreInitialize() { }
}
