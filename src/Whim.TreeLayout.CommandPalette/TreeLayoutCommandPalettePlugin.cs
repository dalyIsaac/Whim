using Whim.CommandPalette;

namespace Whim.TreeLayout.CommandPalette;

/// <summary>
/// This plugin contains commands to interact with the tree layout via the command palette.
/// </summary>
public class TreeLayoutCommandPalettePlugin : IPlugin
{
	private readonly IContext _context;
	private readonly ITreeLayoutPlugin _treeLayoutPlugin;
	private readonly ICommandPalettePlugin _commandLayoutPlugin;

	/// <inheritdoc/>
	public string Name => "whim.tree_layout.command_palette";

	/// <summary>
	/// Creates a new instance of the tree layout command palette plugin.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="treeLayoutPlugin"></param>
	/// <param name="commandPalettePlugin"></param>
	public TreeLayoutCommandPalettePlugin(
		IContext context,
		ITreeLayoutPlugin treeLayoutPlugin,
		ICommandPalettePlugin commandPalettePlugin
	)
	{
		_context = context;
		_treeLayoutPlugin = treeLayoutPlugin;
		_commandLayoutPlugin = commandPalettePlugin;
	}

	/// <inheritdoc />
	public IPluginCommands PluginCommands =>
		new TreeLayoutCommandPalettePluginCommands(_context, this, _treeLayoutPlugin, _commandLayoutPlugin);

	/// <inheritdoc/>
	public void PostInitialize() { }

	/// <inheritdoc/>
	public void PreInitialize() { }
}
