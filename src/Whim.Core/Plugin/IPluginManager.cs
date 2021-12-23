using System.Collections.Generic;

namespace Whim.Core;

public interface IPluginManager
{
	public IEnumerable<IPlugin> AvailablePlugins { get; }

	/// <summary>
	/// Initializes all the plugins, after the rest of the <see cref="IConfigContext"/> has been initialized.
	/// </summary>
	public void Initialize();

	public T RegisterPlugin<T>(T plugin) where T : IPlugin;
}
