using System.Collections.ObjectModel;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// A keybinding. This should be hashable.
/// </summary>
public interface IKeybind
{
	/// <summary>
	/// Modifiers like Alt, Ctrl, and Win.
	/// </summary>
	public KeyModifiers Modifiers { get; }

	/// <summary>
	/// See https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	public VIRTUAL_KEY Key { get; }

	/// <summary>
	/// All the keys which make up this keybind.
	/// </summary>
	public ReadOnlyCollection<string> AllKeys { get; }
}
