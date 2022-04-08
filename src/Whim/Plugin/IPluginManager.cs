using System;
using System.Collections.Generic;

namespace Whim;

public interface IPluginManager : IDisposable
{
	public IEnumerable<IPlugin> AvailablePlugins { get; }

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

	public T RegisterPlugin<T>(T plugin) where T : IPlugin;
}
