using System;
using System.Collections.Generic;

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

	public IKeybindManager AddKeybind(string commandId, IKeybind keybind)
	{
		Logger.Debug($"Adding keybind '{keybind}' for command '{commandId}'");

		if (!_keybindsCommandsMap.ContainsKey(keybind))
		{
			_keybindsCommandsMap.Add(keybind, new List<string>());
		}

		_keybindsCommandsMap[keybind].Add(commandId);
		_commandsKeybindsMap.Add(commandId, keybind);

		return this;
	}

	public ICommand[] GetCommands(IKeybind keybind)
	{
		Logger.Debug($"Getting commands for keybind '{keybind}'");

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

	public IKeybindManager RemoveKeybind(string commandId)
	{
		Logger.Debug($"Removing keybind for command '{commandId}'");

		IKeybind? keybind = TryGetKeybind(commandId);
		if (keybind is not null)
		{
			_commandsKeybindsMap.Remove(commandId);
			_keybindsCommandsMap[keybind].Remove(commandId);
		}

		return this;
	}
}
