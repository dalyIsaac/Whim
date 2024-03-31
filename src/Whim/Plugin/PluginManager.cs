using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Whim;

internal partial class PluginManager : IPluginManager
{
	private readonly object _lockObj = new();
	private readonly IContext _context;
	private readonly CommandManager _commandManager;
	private readonly string _savedStateFilePath;
	private readonly List<IPlugin> _plugins = new();
	private bool _disposedValue;

	[GeneratedRegex(@"^\w+\.\w+(\.\w+)*$")]
	private static partial Regex PluginNameRegex();

	public IReadOnlyCollection<IPlugin> LoadedPlugins => _plugins.AsReadOnly();

	public PluginManager(IContext context, CommandManager commandManager)
	{
		_context = context;
		_commandManager = commandManager;
		_savedStateFilePath = Path.Combine(_context.FileManager.SavedStateDir, "plugins.json");
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
		_context.FileManager.EnsureDirExists(_context.FileManager.SavedStateDir);

		// If the saved plugin state file doesn't yet exist, we don't need to load anything.
		if (!_context.FileManager.FileExists(_savedStateFilePath))
		{
			return;
		}

		using Stream pluginFileStream = _context.FileManager.OpenRead(_savedStateFilePath);

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

	public void Subscribe()
	{
		Logger.Debug("Subscribing each plugin...");

		foreach (IPlugin plugin in _plugins)
		{
			plugin.Subscribe();
		}
	}

	public T AddPlugin<T>(T plugin)
		where T : IPlugin
	{
		using Lock _ = new(_lockObj);
		switch (plugin.Name)
		{
			case ICommandManager.CustomCommandPrefix:
				throw new InvalidOperationException(
					$"Name '{ICommandManager.CustomCommandPrefix}' is reserved for user-defined commands."
				);
			case "whim":
				throw new InvalidOperationException("Name 'whim' is reserved for internal use.");
			case string name when Contains(name):
				throw new InvalidOperationException($"Plugin with name '{plugin.Name}' already exists.");
			case string name when string.IsNullOrWhiteSpace(name):
				throw new InvalidOperationException("Plugin name cannot be empty.");
			case string name when !PluginNameRegex().IsMatch(name):
				throw new InvalidOperationException(
					"Plugin name must be in the format [first](.[second])*. For more, see the regex in PluginManager.cs."
				);
		}

		_plugins.Add(plugin);

		// Add the commands and keybinds.
		foreach (ICommand command in plugin.PluginCommands.Commands)
		{
			_commandManager.AddPluginCommand(command);
		}

		foreach ((string commandId, IKeybind keybind) in plugin.PluginCommands.Keybinds)
		{
			_context.KeybindManager.SetKeybind(commandId, keybind);
		}

		return plugin;
	}

	public bool Contains(string pluginName)
	{
		using Lock _ = new(_lockObj);
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

		_context.FileManager.WriteAllText(_savedStateFilePath, JsonSerializer.Serialize(pluginManagerSavedState));
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
