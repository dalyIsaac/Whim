using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

public static class VirtualKeyExtensions
{
	/// <summary>
	/// Return the <see cref="VIRTUAL_KEY"/> as a string.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public static string GetKeyString(this VIRTUAL_KEY key)
	{
		string keyString = key.ToString();
		keyString = keyString.Replace("VK_", "");

		// Return the keybinding, capitalizing the first letter.
		return string.Concat(keyString[0].ToString().ToUpper(), keyString[1..].ToLower());
	}
}
