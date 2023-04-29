using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

internal class CommandManager : ICommandManager
{
	private readonly Dictionary<string, ICommand> _commands = new();

	public int Count => _commands.Count;

	public bool IsReadOnly => false;

	public void Add(ICommand item)
	{
		if (_commands.ContainsKey(item.Id))
		{
			throw new InvalidOperationException($"Command with id '{item.Id}' already exists.");
		}

		_commands.Add(item.Id, item);
	}

	public ICommand? TryGetCommand(string commandId)
	{
		if (_commands.TryGetValue(commandId, out ICommand? command))
		{
			return command;
		}

		return null;
	}

	public void Clear() => _commands.Clear();

	public bool Contains(ICommand item) => _commands.ContainsKey(item.Id);

	public void CopyTo(ICommand[] array, int arrayIndex) => _commands.Values.CopyTo(array, arrayIndex);

	public IEnumerator<ICommand> GetEnumerator() => _commands.Values.GetEnumerator();

	public bool Remove(ICommand item) => _commands.Remove(item.Id);

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
