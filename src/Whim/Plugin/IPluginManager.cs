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
	public IReadOnlyCollection<IPlugin> LoadedPlugins { get; }

	/// <summary>
	/// Calls all plugins' <see cref="IPlugin.PreInitialize"/> method.
	/// This runs before the rest of the context has been initialized.
	/// </summary>
	public void PreInitialize();

	/// <summary>
	/// Calls all plugins' <see cref="IPlugin.PostInitialize"/> method.
	/// This runs after the rest of the context has been initialized.
	///
	/// <br />
	///
	/// This will also register all the commands for the plugins, specified in
	/// <see cref="IPlugin.Commands"/>.
	/// </summary>
	public void PostInitialize();

	/// <summary>
	/// Adds a plugin.
	/// There's no guarantee that <see cref="PreInitialize"/> will be called before Whim is
	/// initialized. However, <see cref="PostInitialize"/> will still be called after
	/// <see cref="PreInitialize"/>.
	///
	/// <br />
	///
	/// The commands for a plugin specified in <see cref="IPlugin.Commands"/> will be
	/// registered in <see cref="PostInitialize"/>.
	/// </summary>
	/// <param name="plugin">The plugin to add.</param>
	/// <returns>The plugin that was added.</returns>
	public T AddPlugin<T>(T plugin) where T : IPlugin;

	/// <summary>
	/// Whether the plugin manager includes a plugin with the given name.
	/// </summary>
	/// <param name="pluginName">The name of the plugin.</param>
	/// <returns>Whether the plugin manager includes a plugin with the given name.</returns>
	public bool Contains(string pluginName);
}
