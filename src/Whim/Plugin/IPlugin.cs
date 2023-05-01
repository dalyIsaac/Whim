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
	public string Name { get; }

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin before the <see cref="IContext"/> has been initialized.
	/// Put things like event listeners here or adding proxy layout engines
	/// (see <see cref="IWorkspaceManager.AddProxyLayoutEngine(ProxyLayoutEngine)"/>).
	/// </summary>
	public void PreInitialize();

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin after the rest of the <see cref="IContext"/> has been initialized.
	/// Put things which rely on the rest of the context here.
	/// </summary>
	public void PostInitialize();

	/// <summary>
	/// The commands and keybinds for this plugin. These are registered during <see cref="IPluginManager.PreInitialize"/>.
	/// </summary>
	/// <remarks>
	/// Keybindings can be overridden by the user using <see cref="IKeybindManager.Add"/>.
	/// </remarks>
	public IPluginCommands PluginCommands { get; }
}
