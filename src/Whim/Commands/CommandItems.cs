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
	private readonly KeybindsMap _keybindsMap = new();
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

	public bool SetKeybind(string identifier, IKeybind keybind)
	{
		Logger.Debug($"Setting keybind {keybind} for command {identifier}");

		// Get the command.
		ICommand? command = TryGetCommand(identifier);
		if (command == null)
		{
			Logger.Error($"Command {identifier} does not exist");
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

	public bool RemoveKeybind(string identifier)
	{
		Logger.Debug($"Removing keybind for command {identifier}");
		return _keybindsMap.Remove(identifier);
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
			_keybindsMap.Remove(keybind);
		}

		return _identifierCommandMap.Remove(identifier);
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

	public ICommand? TryGetCommand(string identifier)
	{
		Logger.Debug($"Trying to get command \"{identifier}\"");
		return _identifierCommandMap.TryGetValue(identifier, out ICommand? command) ? command : null;
	}

	public ICommand? TryGetCommand(IKeybind keybind)
	{
		Logger.Debug($"Trying to get command bound to keybind {keybind}");
		string? identifier = _keybindsMap.TryGetIdentifier(keybind);
		return identifier != null ? TryGetCommand(identifier) : null;
	}

	public IKeybind? TryGetKeybind(string identifier)
	{
		Logger.Debug($"Trying to get keybind for command \"{identifier}\"");
		return _keybindsMap.TryGetKeybind(identifier);
	}

	public IEnumerator<(ICommand, IKeybind?)> GetEnumerator()
	{
		HashSet<string> processedIdentifiers = new();

		// Iterate over each of the keybinds and associated commands.
		foreach ((IKeybind keybind, string identifier) in _keybindsMap)
		{
			yield return (TryGetCommand(identifier)!, keybind);
			processedIdentifiers.Add(identifier);
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