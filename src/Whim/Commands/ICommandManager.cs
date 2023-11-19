using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// ICommandManager is responsible for managing all the commands for Whim.
/// </summary>
public interface ICommandManager : IEnumerable<ICommand>
{
	/// <summary>
	/// The prefix for all custom commands.
	/// </summary>
	const string CustomCommandPrefix = "whim.custom";

	/// <summary>
	/// Gets the number of commands in the manager.
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Adds a new user-defined command to the manager.
	/// </summary>
	/// <param name="identifier">
	/// The identifier of the command. This will be prefixed with <c>whim.custom.</c>
	/// This must be unique.
	/// </param>
	/// <param name="title">The title of the command.</param>
	/// <param name="callback">
	/// The callback to execute.
	/// This can include triggering a menu to be shown by Whim.CommandPalette,
	/// or to perform some other action.
	/// </param>
	/// <param name="condition">
	/// A condition to determine if the command should be visible, or able to be
	/// executed.
	/// If this is null, the command will always be accessible.
	/// </param>
	void Add(string identifier, string title, Action callback, Func<bool>? condition = null);

	/// <summary>
	/// Tries to get the command with the given identifier.
	/// </summary>
	/// <param name="commandId">The identifier of the command to get</param>
	/// <returns>The command with the given identifier, or null if not found.</returns>
	ICommand? TryGetCommand(string commandId);
}
