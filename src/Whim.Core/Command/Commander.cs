using System.Collections;
using System.Collections.Generic;

namespace Whim.Core;

using CommanderValues = KeyValuePair<CommandType, CommandHandlerDelegate>;

/// <summary>
/// Commander contains the commands and associated handlers for a given class instance, and the
/// instance's child commanders.
/// Each commander can have only a single handler for each <see cref="CommandType"/>.
/// </summary>
public class Commander : IEnumerable<CommanderValues>
{
	/// <summary>
	/// Map of <see cref="CommandType"/> to <see cref="CommandHandlerDelegate"/>.
	/// </summary>
	private readonly Dictionary<CommandType, CommandHandlerDelegate> _ownerCommand = new();

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
	public void Add(CommandType commandType, CommandHandlerDelegate commandHandler)
	{
		Logger.Debug("Adding command {CommandType}", commandType);
		if (_ownerCommand.ContainsKey(commandType))
		{
			Logger.Error("Command {CommandType} already exists", commandType);
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
		Logger.Debug("Adding child commanders");
		_children.AddRange(childCommanders);
	}

	/// <summary>
	/// Execute the provided command. If this <see cref="Commander"/> has an associated handler,
	/// we will run the handler.
	/// </summary>
	/// <param name="command"></param>
	public void ExecuteCommand(ICommand command, int depth = 0)
	{
		Logger.Debug("Executing command {CommandType}", command.CommandType);
		if (_ownerCommand.TryGetValue(command.CommandType, out CommandHandlerDelegate? commandHandler))
		{
			commandHandler(this, new CommandEventArgs(command));
		}

		// Check PreventCascade
		if (command.PreventCascade == true)
		{
			Logger.Debug("Command {CommandType} prevented cascade", command.CommandType);
			return;
		}

		// Check the depth
		if (command.MaxDepth >= depth)
		{
			Logger.Debug("Command's max depth {MaxDepth} reached", command.MaxDepth);
			return;
		}

		Logger.Debug("Searching children.", command.CommandType);
		foreach (Commander? childCommander in _children)
		{
			childCommander.ExecuteCommand(command, depth + 1);
		}
	}
}
