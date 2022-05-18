using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

public static class VirtualKeyExtensions
{
	public static string GetKeyString(this VIRTUAL_KEY key)
	{
		string keyString = key.ToString();
		keyString = keyString.Replace("VK_", "");
		return string.Concat(keyString[0].ToString().ToUpper(), keyString[1..].ToLower());
	}
}
