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
	public IEnumerable<IPlugin> LoadedPlugins { get; }

	/// <summary>
	/// Calls all plugins' <see cref="IPlugin.PreInitialize"/> method.
	/// This runs before the rest of the config context has been initialized.
	/// </summary>
	public void PreInitialize();

	/// <summary>
	/// Calls all plugins' <see cref="IPlugin.PostInitialize"/> method.
	/// This runs after the rest of the config context has been initialized.
	/// </summary>
	public void PostInitialize();

	/// <summary>
	/// Registers a plugin.
	/// There's no guarantee that <see cref="PreInitialize"/> will be called before Whim is
	/// initialized. However, <see cref="PostInitialize"/> will still be called after
	/// <see cref="PreInitialize"/>.
	/// </summary>
	/// <param name="plugin">The plugin to register.</param>
	/// <returns>The plugin that was registered.</returns>
	public T RegisterPlugin<T>(T plugin) where T : IPlugin;
}
