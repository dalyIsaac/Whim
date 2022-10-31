using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// ICommandItems is a collection of commands (though does not implement
/// <see cref="ICollection{T}"/>).
/// It stores the commands and their keybinds for easy access and operation.
/// </summary>
public interface ICommandItemContainer : IEnumerable<CommandItem>
{
	/// <summary>
	/// Adds the <paramref name="command"/> to the collection.
	///
	/// <para/>
	/// Setting <paramref name="keybind"/> will overwrite the existing keybind.
	/// In other words, the last command bound to the keybind will be the one that is bound.
	///
	/// <para/>
	/// A command can have only one keybind.
	///
	/// <para/>
	/// This will throw an exception if there are two different commands with the same identifier.
	/// This is done (in contrast to Whim's general lack of throwing exceptions) as <c>Add</c>
	/// will usually be done during the application's startup.
	/// </summary>
	/// <param name="command">The command to add</param>
	/// <param name="keybind">The keybind to bind the command to</param>
	/// <exception cref="ArgumentException">Thrown if the command's identifier is already bound.</exception>
	public void Add(ICommand command, IKeybind? keybind = null);

	/// <summary>
	/// Sets the existing command (via the <paramref name="commandIdentifier"/>) to
	/// have the given <paramref name="keybind"/>.
	///
	/// <para/>
	/// This will overwrite the existing keybind - command identifier mapping,
	/// and will enforce a one-to-one mapping.
	///
	/// <para/>
	/// This is largely a helper around <see cref="Add(ICommand, IKeybind?)"/>.
	/// </summary>
	/// <param name="commandIdentifier">The identifier of the command to set the keybind for</param>
	/// <param name="keybind">The keybind to bind the command to</param>
	/// <returns>True if the command was found and the keybind was set, false otherwise.</returns>
	public bool SetKeybind(string commandIdentifier, IKeybind keybind);

	/// <summary>
	/// Tries to remove the given keybind. It does not remove the command bound to the keybind.
	/// </summary>
	/// <param name="keybind">The keybind to remove</param>
	/// <returns>If the keybind was removed, returns true. Otherwise, returns false.</returns>
	public bool RemoveKeybind(IKeybind keybind);

	/// <summary>
	/// Tries to remove the keybind bound to the given <paramref name="commandIdentifier"/>.
	/// </summary>
	/// <param name="commandIdentifier">The identifier of the command to remove the keybind for</param>
	/// <returns>If the keybind was removed, returns true. Otherwise, returns false.</returns>
	public bool RemoveKeybind(string commandIdentifier);

	/// <summary>
	/// Tries to remove the given command.
	/// Removing the command will also remove the keybind that is bound to it.
	/// </summary>
	/// <param name="command">The command to remove</param>
	/// <returns>If the command was removed, returns true. Otherwise, returns false.</returns>
	public bool RemoveCommand(ICommand command);

	/// <summary>
	/// Tries to remove the command with the given identifier.
	/// Removing the command will also remove the keybind that is bound to it.
	/// </summary>
	/// <param name="commandIdentifier">The identifier of the command to remove</param>
	/// <returns>If the command was removed, returns true. Otherwise, returns false.</returns>
	public bool RemoveCommand(string commandIdentifier);

	/// <summary>
	/// Clears all commands and keybinds.
	/// </summary>
	public void Clear();

	/// <summary>
	/// Clears all keybinds.
	/// </summary>
	public void ClearKeybinds();

	/// <summary>
	/// Tries to get the command with the given identifier.
	/// </summary>
	/// <param name="commandIdentifier">The identifier of the command to get</param>
	/// <returns>The command with the given identifier, or null if not found.</returns>
	public ICommand? TryGetCommand(string commandIdentifier);

	/// <summary>
	/// Tries to get the given command associated with the given keybind.
	/// </summary>
	/// <param name="keybind">The keybind to get the command from</param>
	/// <returns>The command associated with the given keybind, or null if not found.</returns>
	public ICommand? TryGetCommand(IKeybind keybind);

	/// <summary>
	/// Tries to get the keybind that is bound to the command with the given identifier.
	/// </summary>
	/// <param name="commandIdentifier">The identifier of the command to get the keybind of</param>
	/// <returns>The keybind that is bound to the command with the given identifier, or null if not found.</returns>
	public IKeybind? TryGetKeybind(string commandIdentifier);

	/// <summary>
	/// Tries to get the keybind that is bound to the given command.
	/// </summary>
	/// <param name="command">The command to get the keybind of</param>
	/// <returns>The keybind that is bound to the given command, or null if not found.</returns>
	public IKeybind? TryGetKeybind(ICommand command);
}
