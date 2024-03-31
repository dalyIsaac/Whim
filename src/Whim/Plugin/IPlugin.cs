using System.Text.Json;

namespace Whim;

/// <summary>
/// A plugin for Whim.
/// </summary>
public interface IPlugin
{
	/// <summary>
	/// Unique name of the plugin, in the snake case format of "author_name.plugin_name".
	///
	/// The name must be unique among all plugins. The name will be used in the names of the
	/// commands from <see cref="PluginCommands"/>.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin before the <see cref="IWorkspaceManager"/> and <see cref="IButler"/>
	/// have completed initialization.
	/// This should be used for things like adding proxy layout engines
	/// (see <see cref="IWorkspaceManager.AddProxyLayoutEngine(CreateProxyLayoutEngine)"/>).
	/// </summary>
	void PreInitialize();

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin after the <see cref="IWorkspaceManager"/> and <see cref="IButler"/>
	/// have completed initialization.
	/// Put things which rely on the rest of the context here. Event listeners which don't need
	/// to run on the UI/STA thread can be placed here.
	/// </summary>
	void PostInitialize();

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Put event listeners which need to run on the UI/STA thread here.
	/// </summary>
	void Subscribe();

	/// <summary>
	/// The commands and keybinds for this plugin. These are registered during <see cref="IPluginManager.PreInitialize"/>.
	/// </summary>
	/// <remarks>
	/// Keybindings can be overridden by the user using <see cref="IKeybindManager.SetKeybind(string, IKeybind)"/>.
	/// </remarks>
	IPluginCommands PluginCommands { get; }

	/// <summary>
	/// Load the plugin's state from <paramref name="state"/>.
	/// </summary>
	/// <remarks>
	/// State is loaded after <see cref="PostInitialize"/> and the user's configuration has been loaded.
	/// Thus, be careful on how you interact with the user's configuration.
	/// </remarks>
	/// <param name="state">The plugin's state.</param>
	void LoadState(JsonElement state);

	/// <summary>
	/// Save the plugin's state as a <see cref="JsonElement"/>.
	/// </summary>
	/// <returns>The plugin's state.</returns>
	JsonElement? SaveState();
}
