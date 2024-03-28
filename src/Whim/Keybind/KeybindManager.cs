using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal class KeybindManager : IKeybindManager
{
	private readonly IContext _context;
	private readonly Dictionary<IKeybind, List<string>> _keybindsCommandsMap = new();
	private readonly Dictionary<string, IKeybind> _commandsKeybindsMap = new();

	public KeybindManager(IContext context)
	{
		_context = context;
	}

	private bool _uniqueKeyModifiers = true;
	public bool UnifyKeyModifiers
	{
		get => _uniqueKeyModifiers;
		set
		{
			if (value && _uniqueKeyModifiers == false)
			{
				_uniqueKeyModifiers = true;
				UnifyKeybinds();
			}

			_uniqueKeyModifiers = value;
		}
	}

	private void UnifyKeybinds()
	{
		KeyValuePair<string, IKeybind>[] keybinds = _commandsKeybindsMap.ToArray();
		_commandsKeybindsMap.Clear();
		_keybindsCommandsMap.Clear();

		foreach (KeyValuePair<string, IKeybind> keybind in keybinds)
		{
			SetKeybind(keybind.Key, keybind.Value);
		}
	}

	public void SetKeybind(string commandId, IKeybind keybind)
	{
		Logger.Debug($"Setting keybind '{keybind}' for command '{commandId}'");
		keybind = UnifyKeyModifiers ? keybind.UnifyModifiers() : keybind;

		if (_commandsKeybindsMap.TryGetValue(commandId, out IKeybind? existingKeybind))
		{
			_keybindsCommandsMap[existingKeybind].Remove(commandId);
		}

		if (!_keybindsCommandsMap.TryGetValue(keybind, out List<string>? value))
		{
			value = new List<string>();
			_keybindsCommandsMap.Add(keybind, value);
		}

		value.Add(commandId);
		_commandsKeybindsMap[commandId] = keybind;
	}

	public ICommand[] GetCommands(IKeybind keybind)
	{
		Logger.Debug($"Getting commands for keybind '{keybind}'");
		keybind = UnifyKeyModifiers ? keybind.UnifyModifiers() : keybind;

		if (_keybindsCommandsMap.TryGetValue(keybind, out List<string>? commandIds))
		{
			List<ICommand> commands = new();

			foreach (string commandId in commandIds)
			{
				ICommand? command = _context.CommandManager.TryGetCommand(commandId);

				if (command is not null)
				{
					commands.Add(command);
				}
				else
				{
					Logger.Error($"Command '{commandId}' not found");
				}
			}

			return commands.ToArray();
		}

		return Array.Empty<ICommand>();
	}

	public IKeybind? TryGetKeybind(string commandId)
	{
		Logger.Debug($"Getting keybind for command '{commandId}'");

		return _commandsKeybindsMap.TryGetValue(commandId, out IKeybind? keybind) ? keybind : null;
	}

	public bool Remove(string commandId)
	{
		Logger.Debug($"Removing keybind for command '{commandId}'");

		IKeybind? keybind = TryGetKeybind(commandId);
		if (keybind is null)
		{
			return false;
		}

		_commandsKeybindsMap.Remove(commandId);
		_keybindsCommandsMap[keybind].Remove(commandId);
		return true;
	}

	public void Clear()
	{
		Logger.Debug("Removing all keybinds");
		_commandsKeybindsMap.Clear();
		_keybindsCommandsMap.Clear();
	}
}
