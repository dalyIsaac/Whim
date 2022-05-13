using System;

namespace Whim;

/// <summary>
/// The manager for bindings. This is used as part of <see cref="CommandManager"/>.
/// It is responsible for hooking into windows and handling keybinds.
/// </summary>
internal interface IKeybindManager : IDisposable
{
	/// <summary>
	/// Initialize the keyboard hook.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Adds the keybind and handler to the manager. If the keybind already exists, the handler
	/// is overriden.
	/// </summary>
	/// <param name="command">The command to add</param>
	/// <excpetion cref="ArgumentException">
	/// Thrown if the command's keybind is already in use.
	/// </exception>
	public void Add(ICommand command);

	/// <summary>
	/// Tries to remove the given keybind and associated command.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public bool Remove(IKeybind keybind);

	/// <summary>
	/// Clear all the keybinds and handlers.
	/// </summary>
	public void Clear();

	/// <summary>
	/// Tries to get the command by keybind.
	/// Asking for <see cref="KeyModifiers.LWin"/> will also yield handlers for <see cref="KeyModifiers.Win"/>.
	/// This applies similarly to other left/right modifiers.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public ICommand? TryGet(IKeybind keybind);
}
