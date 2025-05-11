using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="VIRTUAL_KEY"/>.
/// </summary>
public static class VirtualKeyExtensions
{
	private static readonly HashSet<VIRTUAL_KEY> _keyModifiers =
	[
		VIRTUAL_KEY.VK_LCONTROL,
		VIRTUAL_KEY.VK_RCONTROL,
		VIRTUAL_KEY.VK_LSHIFT,
		VIRTUAL_KEY.VK_RSHIFT,
		VIRTUAL_KEY.VK_LALT,
		VIRTUAL_KEY.VK_RALT,
		VIRTUAL_KEY.VK_LWIN,
		VIRTUAL_KEY.VK_RWIN,
	];

	/// <summary>
	/// Return the <see cref="VIRTUAL_KEY"/> as a string.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="unifyKeyModifiers">
	/// Whether to treat key modifiers like `LWin` and `RWin` as the same.
	/// </param>
	/// <returns></returns>
	public static string GetKeyString(this VIRTUAL_KEY key, bool unifyKeyModifiers = false)
	{
		string keyString = key.ToString();
		keyString = keyString.Replace("VK_", "");

		if (unifyKeyModifiers && _keyModifiers.Contains(key))
		{
			keyString = keyString[1..];
		}
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
	/// <seealso cref="GetKeyString(VIRTUAL_KEY, bool)"/>
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
		string enumString = $"VK_{keyString}";

		if (Enum.TryParse(enumString, out VIRTUAL_KEY k))
		{
			key = k;
			return true;
		}

		// Handle unified modifiers.
		enumString = $"VK_L{keyString}";
		if (Enum.TryParse(enumString, out VIRTUAL_KEY k2))
		{
			key = k2;
			return true;
		}

		return false;
	}
}
