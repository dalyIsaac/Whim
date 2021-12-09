using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Core.Binds;

public interface IKeybind
{
	/// <summary>
	/// Optional name of the keybind.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Modifiers like Alt, Ctrl, and Win.
	/// </summary>
	public KeyModifiers Modifiers { get; }

	/// <summary>
	/// See https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	public VIRTUAL_KEY Key { get; }
}
