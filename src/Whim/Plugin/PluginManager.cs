using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Whim;

internal class PluginManager : IPluginManager
{
	private readonly IContext _context;
	private readonly IFileManager _fileManager;
	private readonly CommandManager _commandManager;
	private readonly string _savedStateFilePath;
	private readonly List<IPlugin> _plugins = new();
	private bool _disposedValue;

	public IReadOnlyCollection<IPlugin> LoadedPlugins => _plugins.AsReadOnly();

	public PluginManager(IContext context, IFileManager fileManager, CommandManager commandManager)
	{
		_context = context;
		_fileManager = fileManager;
		_commandManager = commandManager;
		_savedStateFilePath = Path.Combine(_fileManager.SavedStateDir, "plugins.json");
	}

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

		LoadSavedState();
	}

	private void LoadSavedState()
	{
		_fileManager.EnsureDirExists(_fileManager.SavedStateDir);

		// If the saved plugin state file doesn't yet exist, we don't need to load anything.
		if (!_fileManager.FileExists(_savedStateFilePath))
		{
			return;
		}

		using Stream pluginFileStream = _fileManager.OpenRead(_savedStateFilePath);

		PluginManagerSavedState? savedState = null;
		try
		{
			savedState = JsonSerializer.Deserialize<PluginManagerSavedState>(pluginFileStream);
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load plugin state file: {ex}");
			return;
		}

		if (savedState is null)
		{
			return;
		}

		foreach (IPlugin plugin in _plugins)
		{
			if (savedState.Plugins.TryGetValue(plugin.Name, out JsonElement pluginSavedState))
			{
				plugin.LoadState(pluginSavedState);
			}
		}
	}

	public T AddPlugin<T>(T plugin)
		where T : IPlugin
	{
		if (Contains(plugin.Name))
		{
			throw new InvalidOperationException($"Plugin with name '{plugin.Name}' already exists.");
		}

		_plugins.Add(plugin);

		// Add the commands and keybinds.
		foreach (ICommand command in plugin.PluginCommands.Commands)
		{
			_commandManager.AddPluginCommand(command);
		}

		foreach ((string commandId, IKeybind keybind) in plugin.PluginCommands.Keybinds)
		{
			_context.KeybindManager.Add(commandId, keybind);
		}

		return plugin;
	}

	public bool Contains(string pluginName)
	{
		return _plugins.Exists(plugin => plugin.Name == pluginName);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing plugin manager");
				SaveState();

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

	private void SaveState()
	{
		PluginManagerSavedState pluginManagerSavedState = new();

		foreach (IPlugin plugin in _plugins)
		{
			JsonElement? state = plugin.SaveState();
			if (state is JsonElement savedState)
			{
				pluginManagerSavedState.Plugins[plugin.Name] = savedState;
			}
		}

		_fileManager.WriteAllText(_savedStateFilePath, JsonSerializer.Serialize(pluginManagerSavedState));
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}
}
