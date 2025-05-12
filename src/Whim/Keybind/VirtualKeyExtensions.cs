using System.Linq;
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
	/// <param name="unifyKeyModifiers">
	/// Whether to treat key modifiers like `LWin` and `RWin` as the same.
	/// </param>
	/// <returns></returns>
	public static string GetKeyString(this VIRTUAL_KEY key, bool unifyKeyModifiers = false)
	{
		string keyString = key switch
		{
			VIRTUAL_KEY.VK_LCONTROL or VIRTUAL_KEY.VK_RCONTROL when unifyKeyModifiers => "Ctrl",

			VIRTUAL_KEY.VK_LCONTROL => "LCtrl",
			VIRTUAL_KEY.VK_RCONTROL => "RCtrl",

			VIRTUAL_KEY.VK_LSHIFT or VIRTUAL_KEY.VK_RSHIFT when unifyKeyModifiers => "Shift",
			VIRTUAL_KEY.VK_LSHIFT => "LShift",
			VIRTUAL_KEY.VK_RSHIFT => "RShift",

			VIRTUAL_KEY.VK_LALT or VIRTUAL_KEY.VK_RALT when unifyKeyModifiers => "Alt",
			VIRTUAL_KEY.VK_LALT => "LAlt",
			VIRTUAL_KEY.VK_RALT => "RAlt",

			VIRTUAL_KEY.VK_LWIN or VIRTUAL_KEY.VK_RWIN when unifyKeyModifiers => "Win",
			VIRTUAL_KEY.VK_LWIN => "LWin",
			VIRTUAL_KEY.VK_RWIN => "RWin",

			_ => string.Empty,
		};

		if (string.IsNullOrEmpty(keyString))
		{
			string s = key.ToString().Replace("VK_", "");
			return string.Concat(s[0].ToString().ToUpper(), s[1..].ToLower());
		}

		return keyString;
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

	/// <summary>
	/// Tries to get the <see cref="KeyModifiers"/> from a <see cref="VIRTUAL_KEY"/>.
	/// </summary>
	/// <param name="key">
	/// The key to get the modifier from.
	/// </param>
	/// <param name="modifier">
	/// The modifier, if successful.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the key is a modifier; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryGetModifier(this VIRTUAL_KEY key, out KeyModifiers modifier)
	{
		modifier = KeyModifiers.None;

		switch (key)
		{
			case VIRTUAL_KEY.VK_LCONTROL:
				modifier = KeyModifiers.LControl;
				return true;

			case VIRTUAL_KEY.VK_RCONTROL:
				modifier = KeyModifiers.RControl;
				return true;

			case VIRTUAL_KEY.VK_LSHIFT:
				modifier = KeyModifiers.LShift;
				return true;

			case VIRTUAL_KEY.VK_RSHIFT:
				modifier = KeyModifiers.RShift;
				return true;

			case VIRTUAL_KEY.VK_LMENU:
				modifier = KeyModifiers.LAlt;
				return true;

			case VIRTUAL_KEY.VK_RMENU:
				modifier = KeyModifiers.RAlt;
				return true;

			case VIRTUAL_KEY.VK_LWIN:
				modifier = KeyModifiers.LWin;
				return true;

			case VIRTUAL_KEY.VK_RWIN:
				modifier = KeyModifiers.RWin;
				return true;

			default:
				return false;
		}
	}

	/// <summary>
	/// Tries to parse a key modifier from a string.
	/// </summary>
	/// <param name="modifier">
	/// The string to parse. This is case-insensitive.
	/// </param>
	/// <param name="keyModifier">
	/// The parsed key modifier, if successful.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the parse was successful; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryParseKeyModifier(this string modifier, out VIRTUAL_KEY keyModifier)
	{
		keyModifier = modifier.ToUpperInvariant() switch
		{
			"CTRL" => VIRTUAL_KEY.VK_LCONTROL,
			"CONTROL" => VIRTUAL_KEY.VK_LCONTROL,
			"LCTRL" => VIRTUAL_KEY.VK_LCONTROL,
			"LCONTROL" => VIRTUAL_KEY.VK_LCONTROL,

			"RCTRL" => VIRTUAL_KEY.VK_RCONTROL,
			"RCONTROL" => VIRTUAL_KEY.VK_RCONTROL,

			"SHIFT" => VIRTUAL_KEY.VK_LSHIFT,
			"LSHIFT" => VIRTUAL_KEY.VK_LSHIFT,

			"RSHIFT" => VIRTUAL_KEY.VK_RSHIFT,

			"ALT" => VIRTUAL_KEY.VK_LALT,
			"LALT" => VIRTUAL_KEY.VK_LALT,

			"RALT" => VIRTUAL_KEY.VK_RALT,

			"WIN" => VIRTUAL_KEY.VK_LWIN,
			"LWIN" => VIRTUAL_KEY.VK_LWIN,

			"RWIN" => VIRTUAL_KEY.VK_RWIN,

			_ => VIRTUAL_KEY.None,
		};

		return keyModifier != VIRTUAL_KEY.None;
	}

	/// <summary>
	/// Sorts the key modifiers.
	/// </summary>
	/// <param name="modifiers">
	/// The key modifiers to sort.
	/// </param>
	/// <returns>
	/// The sorted key modifiers.
	/// </returns>
	public static ImmutableArray<VIRTUAL_KEY> SortModifiers(IEnumerable<VIRTUAL_KEY> modifiers) =>
		modifiers.Distinct().OrderBy(x => x.GetVirtualKeyIndex()).ToImmutableArray();

	private static int GetVirtualKeyIndex(this VIRTUAL_KEY key) =>
		key switch
		{
			VIRTUAL_KEY.VK_LWIN => 0,
			VIRTUAL_KEY.VK_RWIN => 1,
			VIRTUAL_KEY.VK_LCONTROL => 2,
			VIRTUAL_KEY.VK_RCONTROL => 3,
			VIRTUAL_KEY.VK_LSHIFT => 4,
			VIRTUAL_KEY.VK_RSHIFT => 5,
			VIRTUAL_KEY.VK_LALT => 6,
			VIRTUAL_KEY.VK_RALT => 7,
			_ => 8 + ((int)key),
		};
}
