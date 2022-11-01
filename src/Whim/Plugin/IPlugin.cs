using System.Collections.Generic;

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
	/// commands from <see cref="Commands"/>.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin before the <see cref="IConfigContext"/> has been initialized.
	/// Put things like event listeners here or adding proxy layout engines
	/// (see <see cref="IWorkspaceManager.AddProxyLayoutEngine(ProxyLayoutEngine)"/>).
	/// </summary>
	public void PreInitialize();

	/// <summary>
	/// <b>This method is to be called by the plugin manager.</b>
	/// Initializes the plugin after the rest of the <see cref="IConfigContext"/> has been initialized.
	/// Put things which rely on the rest of the config context here.
	/// </summary>
	public void PostInitialize();

	/// <summary>
	/// Get the default commands for this plugin, and their associated keybinds.
	/// These commands are registered by the <see cref="IPluginManager.PostInitialize"/> method,
	/// during startup.
	/// </summary>
	/// <remarks>
	/// The commands should also be individually accessible via a class <c>PluginCommands</c>,
	/// where <c>Plugin</c> is the name of the plugin.
	/// </remarks>
	public IEnumerable<CommandItem> Commands { get; }
}
