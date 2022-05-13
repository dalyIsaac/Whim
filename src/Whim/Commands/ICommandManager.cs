using System;

namespace Whim;

public interface ICommandManager : IDisposable
{
	/// <summary>
	/// Initialize the keyboard hook.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Adds the command to the manager.
	/// </summary>
	/// <param name="command">The command to add</param>
	/// <exception cref="ArgumentException">
	/// Thrown if the command's identifier or keybind is already in use.
	/// </exception>
	public void Add(ICommand command);

	/// <summary>
	/// Tries to remove the given keybind and associated command.
	/// </summary>
	/// <param name="keybind">The keybind to remove</param>
	/// <returns>If the keybind was removed, returns true. Otherwise, returns false.</returns>
	public bool Remove(IKeybind keybind);

	/// <summary>
	/// Tries to remove the command with the given identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the command to remove</param>
	/// <returns>If the command was removed, returns true. Otherwise, returns false.</returns>
	public bool Remove(string identifier);

	/// <summary>
	/// Clears all commands.
	/// </summary>
	public void Clear();

	/// <summary>
	/// Tries to get the command with the given identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the command to get</param>
	/// <returns>The command with the given identifier, or null if not found.</returns>
	public ICommand? TryGet(string identifier);
}
