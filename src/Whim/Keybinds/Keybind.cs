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
}
