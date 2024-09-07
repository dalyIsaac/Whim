using System.Text;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <inheritdoc />
public readonly record struct Keybind : IKeybind
{
	/// <inheritdoc />
	public KeyModifiers Modifiers { get; }

	/// <inheritdoc />
	public VIRTUAL_KEY Key { get; }

	/// <summary>
	/// Creates a new keybind.
	/// </summary>
	/// <param name="modifiers">The modifiers for the keybind.</param>
	/// <param name="key">The key for the keybind.</param>
	public Keybind(KeyModifiers modifiers, VIRTUAL_KEY key)
	{
		Modifiers = modifiers;
		Key = key;
	}

	/// <inheritdoc />
	public override string ToString() => ToString(false);

	/// <inheritdoc />
	public string ToString(bool unifyKeyModifiers)
	{
		StringBuilder sb = new();
		sb.AppendJoin(" + ", Modifiers.GetParts(unifyKeyModifiers));

		if (sb.Length > 0)
		{
			sb.Append(" + ");
		}

		sb.Append(Key.GetKeyString());
		return sb.ToString();
	}

	/// <summary>
	/// Tries to parse a keybind from a string.
	/// </summary>
	/// <param name="keybind">
	/// The string to parse.
	/// </param>
	/// <returns>
	/// The parsed keybind, if successful; otherwise, <see langword="null"/>.
	/// </returns>
	public static IKeybind? FromString(string keybind)
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

		KeyModifiers modifiers = KeyModifiers.None;
		VIRTUAL_KEY key = VIRTUAL_KEY.None;

		foreach (string part in parts)
		{
			// Some keys are also modifiers, so we need to check for them first.
			if (part.Trim().TryParseKeyModifier(out KeyModifiers modifier))
			{
				modifiers |= modifier;
			}
			else if (part.Trim().TryParseKey(out VIRTUAL_KEY k))
			{
				key = k;
			}
		}

		if (modifiers == KeyModifiers.None || key == VIRTUAL_KEY.None)
		{
			return null;
		}

		return new Keybind(modifiers, key);
	}
}
