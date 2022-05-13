using System.Collections.Generic;

using System;

namespace Whim;

public class CommandManager : ICommandManager
{
	private readonly IKeybindManager _keybindManager;

	private readonly Dictionary<string, ICommand> _commands = new();

	private bool disposedValue;

	public CommandManager()
	{
		_keybindManager = new KeybindManager();
	}

	public void Initialize()
	{
		Logger.Debug("Initializing command manager");
		_keybindManager.Initialize();
	}

	public void Add(ICommand command)
	{
		Logger.Debug($"Adding command {command}");

		if (command.Keybind != null)
		{
			_keybindManager.Add(command);
		}

		if (_commands.ContainsKey(command.Identifier))
		{
			Logger.Error($"Command {command.Identifier} already exists");
			throw new ArgumentException($"Command {command.Identifier} already exists");
		}

		_commands.Add(command.Identifier, command);
	}

	public bool Remove(IKeybind keybind)
	{
		Logger.Debug($"Removing keybind {keybind}");

		// Get the command associated with the keybind.
		ICommand? command = _keybindManager.TryGet(keybind);

		if (command == null)
		{
			Logger.Error($"No command associated with keybind {keybind}");
			return false;
		}

		// Remove the keybind from the keybind manager.
		if (!_keybindManager.Remove(keybind))
		{
			Logger.Error($"Failed to remove keybind {keybind}");
			return false;
		}

		// Remove the command from the command manager.
		if (!_commands.Remove(command.Identifier))
		{
			Logger.Error($"Failed to remove command {command.Identifier}");
			return false;
		}

		return true;
	}

	public bool Remove(string identifier)
	{
		Logger.Debug($"Removing command {identifier}");

		// Get the command associated with the identifier.
		ICommand? command = TryGet(identifier);

		if (command == null)
		{
			Logger.Error($"No command associated with identifier {identifier}");
			return false;
		}

		// Remove the keybind from the keybind manager.
		if (command.Keybind != null && !_keybindManager.Remove(command.Keybind))
		{
			Logger.Error($"Failed to remove keybind {command.Keybind}");
			return false;
		}

		// Remove the command from the command manager.
		if (!_commands.Remove(identifier))
		{
			Logger.Error($"Failed to remove command {identifier}");
			return false;
		}

		return true;
	}

	public void Clear()
	{
		Logger.Debug("Clearing commands");
		_keybindManager.Clear();
		_commands.Clear();
	}

	public ICommand? TryGet(string identifier)
	{
		Logger.Debug($"Getting command {identifier}");

		if (!_commands.TryGetValue(identifier, out ICommand? command))
		{
			Logger.Error($"No command associated with identifier {identifier}");
			return null;
		}

		return command;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_keybindManager.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
