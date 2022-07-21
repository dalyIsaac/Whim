using System;
using System.Collections.Generic;

namespace Whim;

internal class PluginManager : IPluginManager
{
	private readonly List<IPlugin> _plugins = new();
	private bool _disposedValue;

	public IEnumerable<IPlugin> LoadedPlugins => _plugins;

	public void PreInitialize()
	{
		Logger.Debug("Pre-initializing plugin manager...");

		foreach (IPlugin plugin in _plugins)
		{
			plugin.PreInitialize();
		}
	}

	public void PostInitialize()
	{
		Logger.Debug("Post-initializing plugin manager...");

		foreach (IPlugin plugin in _plugins)
		{
			plugin.PostInitialize();
		}
	}

	public T RegisterPlugin<T>(T plugin) where T : IPlugin
	{
		_plugins.Add(plugin);
		return plugin;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing plugin manager");
				foreach (IPlugin plugin in _plugins)
				{
					if (plugin is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}
}
