using System;
using System.Collections.Generic;

namespace Whim;

internal class PluginManager : IPluginManager
{
	private readonly IContext _context;
	private readonly Dictionary<string, IPlugin> _plugins = new();
	private bool _disposedValue;

	public IReadOnlyCollection<IPlugin> LoadedPlugins => _plugins.Values;

	public PluginManager(IContext context)
	{
		_context = context;
	}

	public void PreInitialize()
	{
		Logger.Debug("Pre-initializing plugin manager...");

		foreach (IPlugin plugin in _plugins.Values)
		{
			plugin.PreInitialize();
		}
	}

	public void PostInitialize()
	{
		Logger.Debug("Post-initializing plugin manager...");

		foreach (IPlugin plugin in _plugins.Values)
		{
			plugin.PostInitialize();
			_context.CommandManager.LoadCommands(plugin.Commands);
		}
	}

	public T AddPlugin<T>(T plugin)
		where T : IPlugin
	{
		if (Contains(plugin.Name))
		{
			throw new InvalidOperationException($"Plugin with name '{plugin.Name}' already exists.");
		}

		_plugins.Add(plugin.Name, plugin);
		return plugin;
	}

	public bool Contains(string pluginName)
	{
		return _plugins.ContainsKey(pluginName);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing plugin manager");
				foreach (IPlugin plugin in _plugins.Values)
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
