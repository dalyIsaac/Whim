using System.Linq;
using System.Text;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <inheritdoc />
public readonly struct Keybind : IKeybind, IEquatable<Keybind>
{
	private readonly ImmutableArray<VIRTUAL_KEY> _mods;
	private readonly int _hashCode;

	/// <inheritdoc />
	public IReadOnlyList<VIRTUAL_KEY> Mods => _mods;

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
		: this([.. modifiers.GetKeys()], key) { }

	/// <summary>
	/// Creates a new keybind.
	/// </summary>
	/// <param name="modifiers">
	/// The modifiers for the keybind.
	/// </param>
	/// <param name="key">
	/// The key for the keybind.
	/// </param>
	public Keybind(IEnumerable<VIRTUAL_KEY> modifiers, VIRTUAL_KEY key)
	{
		_mods = VirtualKeyExtensions.SortModifiers(modifiers);
		Key = key;
		Modifiers = KeyModifiers.None;

		foreach (VIRTUAL_KEY mod in _mods)
		{
			if (mod.TryGetModifier(out KeyModifiers modifier))
			{
				Modifiers |= modifier;
			}
		}

		_hashCode = _mods.Aggregate(0, (acc, mod) => acc ^ (int)mod) ^ (int)key;
	}

	/// <inheritdoc />
	public override string ToString() => ToString(false);

	/// <inheritdoc />
	public string ToString(bool unifyKeyModifiers)
	{
		StringBuilder sb = new();

		foreach (VIRTUAL_KEY key in Mods)
		{
			if (sb.Length > 0)
			{
				sb.Append(" + ");
			}

			sb.Append(key.GetKeyString(unifyKeyModifiers));
		}

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

		string[] parts = keybind.Trim().Split('+', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
		{
			return null;
		}

		List<VIRTUAL_KEY> mods = [];
		VIRTUAL_KEY key = VIRTUAL_KEY.None;

		for (int i = 0; i < parts.Length; i++)
		{
			string part = parts[i].Trim();
			bool isLast = i == parts.Length - 1;

			if (part.TryParseKeyModifier(out VIRTUAL_KEY parsedModifier))
			{
				// The last key must not be a modifier.
				if (isLast)
				{
					return null;
				}

				mods.Add(parsedModifier);
				continue;
			}

			if (part.TryParseKey(out VIRTUAL_KEY parsedKey))
			{
				if (isLast)
				{
					key = parsedKey;
					continue;
				}

				mods.Add(parsedKey);
				continue;
			}

			return null;
		}

		if (key == VIRTUAL_KEY.None)
		{
			return null;
		}

		return new Keybind(mods, key);
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		if (obj == null || GetType() != obj.GetType() || obj is not Keybind other)
		{
			return false;
		}

		return Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(Keybind other)
	{
		if (other.Mods.Count != Mods.Count)
		{
			return false;
		}

		for (int idx = 0; idx < Mods.Count; idx += 1)
		{
			if (Mods[idx] != other.Mods[idx])
			{
				return false;
			}
		}

		return Key == other.Key;
	}

	/// <inheritdoc />
	public static bool operator ==(Keybind left, Keybind right) => left.Equals(right);

	/// <inheritdoc />
	public static bool operator !=(Keybind left, Keybind right) => !(left == right);

	/// <inheritdoc />
	public override int GetHashCode() => _hashCode;
}
