using System.Text.Json;
using Whim.CommandPalette;

namespace Whim.TreeLayout.CommandPalette;

/// <summary>
/// This plugin contains commands to interact with the tree layout via the command palette.
/// </summary>
/// <remarks>
/// Creates a new instance of the tree layout command palette plugin.
/// </remarks>
/// <param name="context"></param>
/// <param name="treeLayoutPlugin"></param>
/// <param name="commandPalettePlugin"></param>
public class TreeLayoutCommandPalettePlugin(
	IContext context,
	ITreeLayoutPlugin treeLayoutPlugin,
	ICommandPalettePlugin commandPalettePlugin
) : IPlugin
{
	private readonly IContext _context = context;
	private readonly ITreeLayoutPlugin _treeLayoutPlugin = treeLayoutPlugin;
	private readonly ICommandPalettePlugin _commandLayoutPlugin = commandPalettePlugin;

	/// <summary>
	/// <c>whim.tree_layout.command_palette</c>
	/// </summary>
	public string Name => "whim.tree_layout.command_palette";

	/// <inheritdoc />
	public IPluginCommands PluginCommands =>
		new TreeLayoutCommandPalettePluginCommands(_context, this, _treeLayoutPlugin, _commandLayoutPlugin);

	/// <inheritdoc/>
	public void PostInitialize() { }

	/// <inheritdoc/>
	public void PreInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
