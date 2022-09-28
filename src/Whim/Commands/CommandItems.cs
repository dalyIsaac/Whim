using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// CommandItems is responsible for storing and managing all the commands for the
/// <see cref="CommandManager"/>.
/// </summary>
internal class CommandItems : ICommandItems
{
	private readonly KeybindCommandMap _keybindsMap = new();
	private readonly Dictionary<string, ICommand> _identifierCommandMap = new();

	public void Add(ICommand command, IKeybind? keybind = null)
	{
		Logger.Debug($"Adding command {command}");

		ICommand? existingCommand = TryGetCommand(command.Identifier);
		if (existingCommand != null && command != existingCommand)
		{
			Logger.Error($"Command {command.Identifier} already exists");
			throw new ArgumentException($"Command {command.Identifier} already exists");
		}

		if (keybind != null)
		{
			Logger.Debug($"Adding (or overriding) keybind {keybind}");
			_keybindsMap.Add(keybind, command.Identifier);
		}

		_identifierCommandMap[command.Identifier] = command;
	}

	public bool Add(string commandIdentifier, IKeybind keybind)
	{
		Logger.Debug($"Setting keybind {keybind} for command {commandIdentifier}");

		// Get the command.
		ICommand? command = TryGetCommand(commandIdentifier);
		if (command == null)
		{
			Logger.Error($"Command {commandIdentifier} does not exist");
			return false;
		}

		Add(command, keybind);
		return true;
	}

	public bool RemoveKeybind(IKeybind keybind)
	{
		Logger.Debug($"Removing keybind {keybind}");
		return _keybindsMap.Remove(keybind);
	}

	public bool RemoveKeybind(string commandIdentifier)
	{
		Logger.Debug($"Removing keybind for command {commandIdentifier}");
		return _keybindsMap.Remove(commandIdentifier);
	}

	public bool RemoveCommand(string commandIdentifier)
	{
		Logger.Debug($"Removing command \"{commandIdentifier}\"");

		ICommand? command = TryGetCommand(commandIdentifier);
		if (command == null)
		{
			Logger.Error($"Command \"{commandIdentifier}\" does not exist");
			return false;
		}

		// Get the keybind that is bound to the command.
		IKeybind? keybind = TryGetKeybind(commandIdentifier);

		if (keybind != null)
		{
			_keybindsMap.Remove(keybind);
		}

		return _identifierCommandMap.Remove(commandIdentifier);
	}

	public bool RemoveCommand(ICommand command)
	{
		Logger.Debug($"Removing command {command}");
		return RemoveCommand(command.Identifier);
	}

	public void Clear()
	{
		Logger.Debug("Clearing commands");
		_identifierCommandMap.Clear();
		ClearKeybinds();
	}

	public void ClearKeybinds()
	{
		Logger.Debug("Clearing keybinds");
		_keybindsMap.Clear();
	}

	public ICommand? TryGetCommand(string commandIdentifier)
	{
		Logger.Debug($"Trying to get command \"{commandIdentifier}\"");
		return _identifierCommandMap.TryGetValue(commandIdentifier, out ICommand? command) ? command : null;
	}

	public ICommand? TryGetCommand(IKeybind keybind)
	{
		Logger.Debug($"Trying to get command bound to keybind {keybind}");
		string? commandIdentifier = _keybindsMap.TryGetIdentifier(keybind);
		return commandIdentifier != null ? TryGetCommand(commandIdentifier) : null;
	}

	public IKeybind? TryGetKeybind(string commandIdentifier)
	{
		Logger.Debug($"Trying to get keybind for command \"{commandIdentifier}\"");
		return _keybindsMap.TryGetKeybind(commandIdentifier);
	}

	public IKeybind? TryGetKeybind(ICommand command)
	{
		Logger.Debug($"Trying to get keybind for command {command}");
		return TryGetKeybind(command.Identifier);
	}

	public IEnumerator<(ICommand, IKeybind?)> GetEnumerator()
	{
		HashSet<string> processedIdentifiers = new();

		// Iterate over each of the keybinds and associated commands.
		foreach ((IKeybind keybind, string commandIdentifier) in _keybindsMap)
		{
			yield return (TryGetCommand(commandIdentifier)!, keybind);
			processedIdentifiers.Add(commandIdentifier);
		}

		// Iterate over each of the commands without an associated keybind.
		foreach (KeyValuePair<string, ICommand> pair in _identifierCommandMap)
		{
			if (!processedIdentifiers.Contains(pair.Key))
			{
				yield return (pair.Value, null);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
