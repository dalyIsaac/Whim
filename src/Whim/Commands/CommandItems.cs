using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// CommandItems is responsible for storing and managing all the commands for the
/// <see cref="CommandManager"/>.
/// </summary>
internal class CommandItems : ICommandItems
{
	private readonly Dictionary<IKeybind, string> _keybindIdentifierMap = new();
	private readonly Dictionary<string, ICommand> _identifierCommandMap = new();

	public void Add(ICommand command, IKeybind? keybind = null)
	{
		Logger.Debug($"Adding command {command}");

		if (keybind != null)
		{
			Logger.Debug($"Adding (or overriding) keybind {keybind}");
			_keybindIdentifierMap[keybind] = command.Identifier;
		}

		ICommand? existingCommand = TryGetCommand(command.Identifier);

		if (existingCommand != null && command != existingCommand)
		{
			Logger.Error($"Command {command.Identifier} already exists");
			throw new ArgumentException($"Command {command.Identifier} already exists");
		}

		_identifierCommandMap[command.Identifier] = command;
	}

	public bool Bind(string identifier, IKeybind keybind)
	{
		Logger.Debug($"Binding command \"{identifier}\" to keybind {keybind}");
		ICommand? command = TryGetCommand(identifier);

		if (command == null)
		{
			Logger.Error($"Command with identifier \"{identifier}\" does not exist");
			return false;
		}

		_keybindIdentifierMap[keybind] = identifier;
		return true;
	}

	public bool Remove(IKeybind keybind)
	{
		Logger.Debug($"Removing keybind {keybind}");
		return _keybindIdentifierMap.Remove(keybind);
	}

	public bool Remove(string identifier)
	{
		Logger.Debug($"Removing command \"{identifier}\"");

		ICommand? command = TryGetCommand(identifier);
		if (command == null)
		{
			Logger.Error($"Command \"{identifier}\" does not exist");
			return false;
		}

		// Get the keybind that is bound to the command.
		IKeybind? keybind = TryGetKeybind(identifier);

		if (keybind != null)
		{
			if (_keybindIdentifierMap.Remove(keybind))
			{
				Logger.Debug($"Removed keybind {keybind}");
			}
			else
			{
				Logger.Error($"Failed to remove keybind {keybind}");
				return false;
			}
		}

		return _identifierCommandMap.Remove(identifier);
	}

	public void Clear()
	{
		Logger.Debug("Clearing commands");
		_keybindIdentifierMap.Clear();
		_identifierCommandMap.Clear();
	}

	public void ClearKeybinds()
	{
		Logger.Debug("Clearing keybinds");
		_keybindIdentifierMap.Clear();
	}

	public ICommand? TryGetCommand(string identifier)
	{
		Logger.Debug($"Trying to get command \"{identifier}\"");
		return _identifierCommandMap.TryGetValue(identifier, out ICommand? command) ? command : null;
	}

	public ICommand? TryGetCommand(IKeybind keybind)
	{
		Logger.Debug($"Trying to get command bound to keybind {keybind}");
		if (!_keybindIdentifierMap.TryGetValue(keybind, out string? identifierValue))
		{
			return null;
		}

		return TryGetCommand(identifierValue);
	}

	public IKeybind? TryGetKeybind(string identifier)
	{
		Logger.Debug($"Trying to get keybind for command \"{identifier}\"");

		foreach (KeyValuePair<IKeybind, string> pair in _keybindIdentifierMap)
		{
			if (pair.Value == identifier)
			{
				return pair.Key;
			}
		}

		return null;
	}
}