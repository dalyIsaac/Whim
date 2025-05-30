using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// <c>IKeybindManager</c> is responsible for managing all the keybinds for Whim.
/// </summary>
public interface IKeybindManager
{
	/// <summary>
	/// Whether to treat key modifiers like `LWin` and `RWin` as the same.
	/// For key modifiers which have a left and right variant, this will treat them as the same.
	/// Defaults to <c>true</c>.
	/// </summary>
	/// <remarks>
	/// When this is set to <c>true</c>, all of the existing keybinds will be re-added in a unified
	/// form.
	/// All new keybinds will also be unified.
	/// </remarks>
	bool UnifyKeyModifiers { get; set; }

	/// <summary>
	/// All the modifiers which are currently being used.
	/// </summary>
	IEnumerable<VIRTUAL_KEY> Modifiers { get; }

	/// <summary>
	/// Sets a keybind.
	/// If a keybind already exists for the given command, it will be overwritten.
	/// </summary>
	/// <remarks>
	/// Keybinds can have multiple commands bound to them.
	/// Commands can have a single keybind.
	/// </remarks>
	/// <param name="commandId">The identifier of the command to bind to.</param>
	/// <param name="keybind">The keybind to set.</param>
	void SetKeybind(string commandId, IKeybind keybind);

	/// <summary>
	/// Removes a keybind.
	/// </summary>
	/// <param name="commandId">The identifier of the command to remove the keybind from.</param>
	/// <returns><c>true</c> if the keybind was removed; otherwise, <c>false</c>.</returns>
	bool Remove(string commandId);

	/// <summary>
	/// Clears/removes all keybinds.
	/// </summary>
	void Clear();

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
