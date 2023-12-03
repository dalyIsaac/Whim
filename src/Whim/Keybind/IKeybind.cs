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
	public const KeyModifiers Win = KeyModifiers.LWin;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Alt</c> command.
	/// </summary>
	public const KeyModifiers WinAlt = KeyModifiers.LWin | KeyModifiers.LAlt;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Shift</c> command.
	/// </summary>
	public const KeyModifiers WinShift = KeyModifiers.LWin | KeyModifiers.LShift;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> command.
	/// </summary>
	public const KeyModifiers WinCtrl = KeyModifiers.LWin | KeyModifiers.LControl;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> + <c>Shift</c> command.
	/// </summary>
	public const KeyModifiers WinCtrlShift = KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift;

	/// <summary>
	/// Modifiers like Alt, Ctrl, and Win.
	/// </summary>
	KeyModifiers Modifiers { get; }

	/// <summary>
	/// See https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	VIRTUAL_KEY Key { get; }

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
