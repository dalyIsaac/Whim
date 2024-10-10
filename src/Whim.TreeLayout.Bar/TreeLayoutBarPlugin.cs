using System.Text.Json;
using Microsoft.UI.Xaml.Controls;
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
	public BarComponent CreateComponent() => new TreeLayoutComponent(_plugin);

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}

/// <summary>
/// The tree layout engine component for the bar.
/// </summary>
public record TreeLayoutComponent(ITreeLayoutPlugin Plugin) : BarComponent
{
	/// <inheritdoc/>
	public override UserControl CreateWidget(IContext context, IMonitor monitor, Microsoft.UI.Xaml.Window window) =>
		new TreeLayoutEngineWidget(context, Plugin, monitor, window);
}
