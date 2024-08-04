using System.Text.Json;
using Whim.Bar;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// This plugin contains the tree layout engine widget for the <see cref="IBarPlugin"/>.
/// </summary>
/// <remarks>
/// Create a new instance of the <see cref="TreeLayoutBarPlugin"/> class.
/// </remarks>
/// <param name="plugin"></param>
public class TreeLayoutBarPlugin(ITreeLayoutPlugin plugin) : IPlugin
{
	private readonly ITreeLayoutPlugin _plugin = plugin;

	/// <summary>
	/// <c>whim.tree_layout.bar</c>
	/// </summary>
	public string Name => "whim.tree_layout.bar";

	/// <inheritdoc/>
	public IPluginCommands PluginCommands => new PluginCommands(Name);

	/// <inheritdoc/>
	public void PreInitialize() { }

	/// <inheritdoc/>
	public void PostInitialize() { }

	/// <summary>
	/// Create the tree layout engine bar component.
	/// </summary>
	/// <returns></returns>
	public BarComponent CreateComponent()
	{
		return new BarComponent(
			(context, monitor, window) => new TreeLayoutEngineWidget(context, _plugin, monitor, window)
		);
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
