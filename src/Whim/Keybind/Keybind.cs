using System.Linq;
using System.Text;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <inheritdoc />
public struct Keybind : IKeybind
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
	public Keybind(KeyModifiers modifiers, VIRTUAL_KEY key) : this([.. modifiers.GetKeys()], key)
	{ }

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
		IEnumerable<VIRTUAL_KEY> sortedModifiers = modifiers
			.Where(x => x != VIRTUAL_KEY.None)
			.Distinct()
			.OrderBy(x => x);

		_mods = [.. sortedModifiers];
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
		sb.AppendJoin(" + ", Modifiers.GetParts(unifyKeyModifiers));

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

		string[] parts = keybind.Split('+', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
		{
			return null;
		}

		List<VIRTUAL_KEY> mods = [];
		VIRTUAL_KEY key = VIRTUAL_KEY.None;

		for (int i = 0; i < parts.Length - 1; i++)
		{
			if (!parts[i].Trim().TryParseKey(out VIRTUAL_KEY k))
			{
				continue;
			}

			if (i == parts.Length - 1)
			{
				key = k;
			}
			else
			{
				mods.Add(k);
			}
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
	public override int GetHashCode() => _hashCode;
}
