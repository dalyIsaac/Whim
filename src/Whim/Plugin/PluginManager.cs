using System.Collections.Generic;

namespace Whim;

public class PluginManager : IPluginManager
{
	private readonly IConfigContext _configContext;
	private List<IPlugin> _plugins = new();

	public IEnumerable<IPlugin> AvailablePlugins { get => _plugins; }

	public PluginManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	public void Initialize()
	{
		Logger.Debug("Initializing plugin manager...");

		foreach (var plugin in _plugins)
		{
			plugin.Initialize();
		}
	}

	public T RegisterPlugin<T>(T plugin) where T : IPlugin
	{
		_plugins.Add(plugin);
		return plugin;
	}
}
