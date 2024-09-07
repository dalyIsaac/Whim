using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="VIRTUAL_KEY"/>.
/// </summary>
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

	/// <summary>
	/// Try to parse a key from a string.
	/// </summary>
	/// <param name="keyString">
	/// The string to parse.
	/// </param>
	/// <param name="key">
	/// The parsed key.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the key was parsed successfully; otherwise, <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// This method is case-insensitive.
	/// </remarks>
	/// <seealso cref="GetKeyString(VIRTUAL_KEY)"/>
	/// <seealso cref="VIRTUAL_KEY"/>
	public static bool TryParseKey(this string keyString, out VIRTUAL_KEY key)
	{
		key = VIRTUAL_KEY.None;

		if (string.IsNullOrWhiteSpace(keyString))
		{
			return false;
		}

		keyString = keyString.ToUpperInvariant();
		keyString = keyString.Replace(" ", "");

		if (keyString.Length == 1)
		{
			keyString = $"VK_{keyString}";
		}
		else
		{
			keyString = $"VK_{keyString[0].ToString().ToUpper()}{keyString[1..].ToLower()}";
		}

		if (Enum.TryParse(keyString, out VIRTUAL_KEY k))
		{
			key = k;
			return true;
		}

		return false;
	}
}
