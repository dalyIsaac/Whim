namespace Whim;

/// <summary>
/// <c>IKeybindManager</c> is responsible for managing all the keybinds for Whim.
/// </summary>
public interface IKeybindManager
{
	/// <summary>
	/// Adds a keybind.
	/// </summary>
	/// <remarks>
	/// Keybinds can have multiple commands bound to them.
	/// </remarks>
	/// <param name="commandId">The identifier of the command to bind to.</param>
	/// <param name="keybind">The keybind to add.</param>
	IKeybindManager AddKeybind(string commandId, IKeybind keybind);

	/// <summary>
	/// Removes a keybind.
	/// </summary>
	/// <param name="commandId">The identifier of the command to remove the keybind from.</param>
	IKeybindManager RemoveKeybind(string commandId);

	/// <summary>
	/// Gets the keybind for the given command.
	/// </summary>
	/// <param name="commandId">The identifier of the command to get the keybind for.</param>
	/// <returns>The keybind for the given command.</returns>
	IKeybind? TryGetKeybind(string commandId);

	/// <summary>
	/// Gets all the commands bound to the given keybind.
	/// </summary>
	/// <param name="keybind">The keybind to get the commands for.</param>
	/// <returns>An array of commands bound to the given keybind.</returns>
	ICommand[] GetCommands(IKeybind keybind);
}
