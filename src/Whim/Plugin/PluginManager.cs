using System;
using System.Collections.Generic;

namespace Whim;

public class PluginManager : IPluginManager
{
	private readonly IConfigContext _configContext;
	private readonly List<IPlugin> _plugins = new();
	private bool disposedValue;

	public IEnumerable<IPlugin> AvailablePlugins { get => _plugins; }

	public PluginManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	public void Initialize()
	{
		Logger.Debug("Initializing plugin manager...");

		foreach (IPlugin plugin in _plugins)
		{
			plugin.Initialize();
		}
	}

	public T RegisterPlugin<T>(T plugin) where T : IPlugin
	{
		_plugins.Add(plugin);
		return plugin;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				foreach (IPlugin plugin in _plugins)
				{
					if (plugin is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}
}
