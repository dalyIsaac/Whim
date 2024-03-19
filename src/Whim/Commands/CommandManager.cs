using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

internal class CommandManager : ICommandManager
{
	private readonly object _lockObj = new();
	private readonly Dictionary<string, ICommand> _commands = new();

	public int Count => _commands.Count;

	/// <summary>
	/// Add a command from a plugin.
	/// </summary>
	/// <param name="item"></param>
	/// <exception cref="InvalidOperationException"></exception>
	internal void AddPluginCommand(ICommand item)
	{
		using Lock _ = new(_lockObj);
		AddPluginFn(item);
	}

	private void AddPluginFn(ICommand item)
	{
		if (_commands.ContainsKey(item.Id))
		{
			throw new InvalidOperationException($"Command with id '{item.Id}' already exists.");
		}

		_commands.Add(item.Id, item);
	}

	public void Add(string identifier, string title, Action callback, Func<bool>? condition = null)
	{
		using Lock _ = new(_lockObj);
		AddFn(identifier, title, callback, condition);
	}

	private void AddFn(string identifier, string title, Action callback, Func<bool>? condition)
	{
		if (!identifier.StartsWith(ICommandManager.CustomCommandPrefix))
		{
			identifier = $"{ICommandManager.CustomCommandPrefix}.{identifier}";
		}

		AddPluginCommand(new Command(identifier, title, callback, condition));
	}

	public ICommand? TryGetCommand(string commandId)
	{
		using Lock _ = new(_lockObj);
		if (_commands.TryGetValue(commandId, out ICommand? command))
		{
			return command;
		}

		return null;
	}

	public IEnumerator<ICommand> GetEnumerator() => _commands.Values.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
