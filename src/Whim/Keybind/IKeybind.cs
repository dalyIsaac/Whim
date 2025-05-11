using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// A keybinding. This should be hashable.
/// </summary>
public interface IKeybind
{
	/// <summary>
	/// The value for the key modifier <c>Win</c> command.
	/// </summary>
	[Obsolete("Use new KeyModifiers(VIRTUAL_KEY.VK_LWIN, otherKey1, otherKey2, otherKeyN) instead.")]
	const KeyModifiers Win = KeyModifiers.LWin;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Alt</c> command.
	/// </summary>
	[Obsolete("Use new KeyModifiers(VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LALT, otherKey1, otherKeyN) instead.")]
	const KeyModifiers WinAlt = KeyModifiers.LWin | KeyModifiers.LAlt;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Shift</c> command.
	/// </summary>
	[Obsolete("Use new KeyModifiers(VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LSHIFT, otherKey1, otherKeyN) instead.")]
	const KeyModifiers WinShift = KeyModifiers.LWin | KeyModifiers.LShift;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> command.
	/// </summary>
	[Obsolete("Use new KeyModifiers(VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LCONTROL, otherKey1, otherKeyN) instead.")]
	const KeyModifiers WinCtrl = KeyModifiers.LWin | KeyModifiers.LControl;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> + <c>Shift</c> command.
	/// </summary>
	[Obsolete(
		"Use new KeyModifiers(VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT, otherKey1, otherKeyN) instead."
	)]
	const KeyModifiers WinCtrlShift = KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift;

	/// <summary>
	/// Modifiers like Alt, Ctrl, and Win.
	/// </summary>
	[Obsolete("Use Keys instead.")]
	KeyModifiers Modifiers { get; }

	/// <summary>
	/// See https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	[Obsolete("Use Keys instead.")]
	VIRTUAL_KEY Key { get; }

	/// <summary>
	/// All the keys in the keybind.
	/// </summary>
	/// <remarks>
	/// Whim does not distinguish between key modifiers and keys. This means that you have
	/// <c>A + B</c> as a keybind, in addition to the typical <c>Win + A</c>.
	/// </remarks>
	IEnumerable<VIRTUAL_KEY> Keys { get; }

	/// <summary>
	/// Returns a string representation of the keybind.
	/// </summary>
	/// <param name="unifyKeyModifiers">
	/// Whether to treat key modifiers like `LWin` and `RWin` as the same.
	/// See <see cref="IKeybindManager.UnifyKeyModifiers"/>.
	/// </param>
	/// <returns></returns>
	string ToString(bool unifyKeyModifiers);
}
