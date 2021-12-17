using System.Collections;
using System.Collections.Generic;

namespace Whim.Core;

using CommanderValues = KeyValuePair<CommandType, CommandEventHandler>;

/// <summary>
/// Commander contains the commands and associated handlers for a given class instance, and the
/// instance's child commanders.
/// Each commander can have only a single handler for each <see cref="CommandType"/>.
/// </summary>
public class Commander : IEnumerable<CommanderValues>
{
	/// <summary>
	/// Map of <see cref="CommandType"/> to <see cref="CommandEventHandler"/>.
	/// </summary>
	private readonly Dictionary<CommandType, CommandEventHandler> _ownerCommand = new();

	/// <summary>
	/// The child commanders.
	/// </summary>
	private readonly List<Commander> _children = new();

	public IEnumerator<CommanderValues> GetEnumerator() => _ownerCommand.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Add a new command.
	/// </summary>
	/// <param name="commandType"></param>
	/// <param name="commandHandler"></param>
	/// <exception cref="System.Exception"></exception>
	public void Add(CommandType commandType, CommandEventHandler commandHandler)
	{
		Logger.Debug($"Adding command {commandType}");
		if (_ownerCommand.ContainsKey(commandType))
		{
			Logger.Error($"Command {commandType} already exists");
			throw new System.Exception($"Command {commandType} already exists");
		}

		_ownerCommand.Add(commandType, commandHandler);
	}

	/// <summary>
	/// Add a new child commander.
	/// </summary>
	/// <param name="childCommanders"></param>
	public void Add(params Commander[] childCommanders)
	{
		Logger.Debug($"Adding child commanders");
		_children.AddRange(childCommanders);
	}

	/// <summary>
	/// Execute the provided command. If this <see cref="Commander"/> has an associated handler,
	/// we will run the handler.
	/// </summary>
	/// <param name="command"></param>
	public void ExecuteCommand(ICommand command, int depth = 0)
	{
		Logger.Debug($"Executing command {command}");
		if (_ownerCommand.TryGetValue(command.CommandType, out CommandEventHandler? commandHandler))
		{
			commandHandler(this, new CommandEventArgs(command));
		}

		// Check PreventCascade
		if (command.PreventCascade == true)
		{
			Logger.Debug($"Command {command} prevented cascade");
			return;
		}

		// Check the depth
		if (command.MaxDepth >= depth)
		{
			Logger.Debug($"Command's max depth {command.MaxDepth} reached");
			return;
		}

		Logger.Debug($"Searching children for {command}");
		foreach (Commander? childCommander in _children)
		{
			childCommander.ExecuteCommand(command, depth + 1);
		}
	}
}
