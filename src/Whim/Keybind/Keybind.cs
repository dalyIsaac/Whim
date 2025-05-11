using System.Text;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <inheritdoc />
public readonly record struct Keybind : IKeybind
{
	/// <inheritdoc />
	[Obsolete("Use Keys instead.")]
	public KeyModifiers Modifiers { get; }

	/// <inheritdoc />
	[Obsolete("Use Keys instead.")]
	public VIRTUAL_KEY Key { get; }

	private readonly ImmutableArray<VIRTUAL_KEY> _keys;

	/// <inheritdoc />
	public IEnumerable<VIRTUAL_KEY> Keys => _keys;

	/// <summary>
	/// Creates a new keybind.
	/// </summary>
	/// <param name="modifiers">The modifiers for the keybind.</param>
	/// <param name="key">The key for the keybind.</param>
	[Obsolete("Provide VIRTUAL_KEYs instead of KeyModifiers.")]
	public Keybind(KeyModifiers modifiers, VIRTUAL_KEY key)
	{
		_keys = [.. modifiers.GetVirtualKeys(), key];
	}

	/// <summary>
	/// Creates a new keybind.
	/// </summary>
	/// <param name="key">
	/// One of the keys for the keybind.
	/// </param>
	/// <param name="otherKeys">
	/// Any other keys for the keybind.
	/// </param>
	public Keybind(VIRTUAL_KEY key, params VIRTUAL_KEY[] otherKeys)
	{
		_keys = [key, .. otherKeys];
	}

	/// <inheritdoc />
	public override string ToString() => ToString(false);

	/// <inheritdoc />
	public string ToString(bool unifyKeyModifiers)
	{
		StringBuilder sb = new();

		foreach (VIRTUAL_KEY key in Keys)
		{
			if (sb.Length > 0)
			{
				sb.Append(" + ");
			}

			sb.Append(key.GetKeyString(unifyKeyModifiers));
		}

		return sb.ToString();
	}

	/// <summary>
	/// Tries to parse a keybind from a string.
	/// </summary>
	/// <param name="keybind">
	/// The string to parse.
	/// </param>
	/// <param name="unifyKeyModifiers">
	/// Whether to treat key modifiers like `LWin` and `RWin` as the same.
	/// See <see cref="IKeybindManager.UnifyKeyModifiers"/>.
	/// </param>
	/// <returns>
	/// The parsed keybind, if successful; otherwise, <see langword="null"/>.
	/// </returns>
	public static IKeybind? FromString(string keybind, bool unifyKeyModifiers = false)
	{
		if (string.IsNullOrWhiteSpace(keybind))
		{
			return null;
		}

		string[] parts = keybind.Split('+', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
		{
			return null;
		}

		VIRTUAL_KEY firstKey = VIRTUAL_KEY.None;
		List<VIRTUAL_KEY> keys = [];

		foreach (string part in parts)
		{
			if (!part.Trim().TryParseKey(out VIRTUAL_KEY k))
			{
				continue;
			}

			if (firstKey == VIRTUAL_KEY.None)
			{
				firstKey = k;
			}
			else
			{
				keys.Add(k);
			}
		}

		if (firstKey == VIRTUAL_KEY.None)
		{
			return null;
		}

		return new Keybind(firstKey, [.. keys]);
	}
}
