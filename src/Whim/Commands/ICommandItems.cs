using System;

namespace Whim;

/// <summary>
/// ICommandItems is a collection of commands (though does not implement
/// <see cref="System.Collections.Generic.ICollection{T}"/>).
/// It stores the commands and their keybinds for easy access and operation.
/// </summary>
public interface ICommandItems
{
	/// <summary>
	/// Adds the <paramref name="command"/> to the collection.
	/// If <paramref name="keybind"/> is not null and is already bound, it will be overwritten.
	/// In other words, the last command bound to the keybind will be the one that is bound.
	/// </summary>
	/// <param name="command">The command to add</param>
	/// <param name="keybind">The keybind to bind the command to</param>
	/// <exception cref="ArgumentException">Thrown if the command's identifier is already bound.</exception>
	public void Add(ICommand command, IKeybind? keybind = null);

	/// <summary>
	/// Bind the command with the given identifier to the given keybind.
	/// If the keybind is already bound, it will be overwritten.
	/// In other words, the last command bound to the keybind will be the one that is bound.
	/// </summary>
	/// <param name="identifier">The identifier of the command to bind</param>
	/// <param name="keybind">The keybind to bind the command to</param>
	/// <returns>False if the identifier does not have a command bound to it. True otherwise.</returns>
	public bool Bind(string identifier, IKeybind keybind);

	/// <summary>
	/// Tries to remove the given keybind and associated command.
	/// </summary>
	/// <param name="keybind">The keybind to remove</param>
	/// <returns>If the keybind was removed, returns true. Otherwise, returns false.</returns>
	public bool Remove(IKeybind keybind);

	/// <summary>
	/// Tries to remove the command with the given identifier.
	/// Removing the command will also remove the keybind that is bound to it.
	/// </summary>
	/// <param name="identifier">The identifier of the command to remove</param>
	/// <returns>If the command was removed, returns true. Otherwise, returns false.</returns>
	public bool Remove(string identifier);

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
	/// <param name="identifier">The identifier of the command to get</param>
	/// <returns>The command with the given identifier, or null if not found.</returns>
	public ICommand? TryGetCommand(string identifier);

	/// <summary>
	/// Tries to get the given command associated with the given keybind.
	/// </summary>
	/// <param name="keybind">The keybind to get the command from</param>
	/// <returns>The command associated with the given keybind, or null if not found.</returns>
	public ICommand? TryGetCommand(IKeybind keybind);

	/// <summary>
	/// Tries to get the keybind that is bound to the command with the given identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the command to get the keybind of</param>
	/// <returns>The keybind that is bound to the command with the given identifier, or null if not found.</returns>
	public IKeybind? TryGetKeybind(string identifier);
}
