using System.Collections.Generic;

namespace Whim;

/// <summary>
/// ICommandManager is responsible for managing all the commands for Whim.
/// </summary>
public interface ICommandManager : ICollection<ICommand>
{
	/// <summary>
	/// Tries to get the command with the given identifier.
	/// </summary>
	/// <param name="commandId">The identifier of the command to get</param>
	/// <returns>The command with the given identifier, or null if not found.</returns>
	public ICommand? TryGetCommand(string commandId);
}
