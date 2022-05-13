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
}
