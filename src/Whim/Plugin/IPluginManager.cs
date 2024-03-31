using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Manager for the loaded plugins.
/// </summary>
public interface IPluginManager : IDisposable
{
	/// <summary>
	/// The currently loaded plugins.
	/// </summary>
	IReadOnlyCollection<IPlugin> LoadedPlugins { get; }

	/// <summary>
	/// Calls all plugins' <see cref="IPlugin.PreInitialize"/> methods.
	/// This runs before the rest of the context has been initialized.
	/// </summary>
	void PreInitialize();

	/// <summary>
	/// Performs the following:
	///
	/// <list type="number">
	///
	/// <item>
	/// <description>
	/// Calls <see cref="IPlugin.PostInitialize"/> for all plugins.
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// Loads the state of all plugins from <see cref="IPlugin.LoadState"/>, from the last JSON saved state.
	/// </description>
	/// </item>
	///
	/// </list>
	/// </summary>
	void PostInitialize();

	/// <summary>
	/// Calls all plugins' <see cref="IPlugin.Subscribe"/> methods. This runs on the UI thread.
	/// </summary>
	void Subscribe();

	/// <summary>
	/// Adds a plugin, registers its commands and keybinds from <see cref="IPlugin.PluginCommands"/>.
	///
	/// <br />
	///
	/// There is no guarantee that <see cref="PreInitialize"/> will be called before Whim is
	/// initialized. However, <see cref="PostInitialize"/> will still be called after
	/// <see cref="PreInitialize"/>.
	///
	/// </summary>
	/// <param name="plugin">The plugin to add.</param>
	/// <returns>The plugin that was added.</returns>
	T AddPlugin<T>(T plugin)
		where T : IPlugin;

	/// <summary>
	/// Whether the plugin manager includes a plugin with the given name.
	/// </summary>
	/// <param name="pluginName">The name of the plugin.</param>
	/// <returns>Whether the plugin manager includes a plugin with the given name.</returns>
	bool Contains(string pluginName);
}
