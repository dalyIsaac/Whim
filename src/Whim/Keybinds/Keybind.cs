using System.Collections.Immutable;
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
	/// Saved representation of the keybind as a string.
	/// </summary>
	private readonly string _allKeysStr;

	/// <summary>
	/// Creates a new keybind.
	/// </summary>
	/// <param name="modifiers">The modifiers for the keybind.</param>
	/// <param name="key">The key for the keybind.</param>
	public Keybind(KeyModifiers modifiers, VIRTUAL_KEY key)
	{
		Modifiers = modifiers;
		Key = key;

		ImmutableArray<string>.Builder allKeys = ImmutableArray.CreateBuilder<string>();
		allKeys.AddRange(Modifiers.GetParts());
		allKeys.Add(Key.GetKeyString());

		_allKeysStr = string.Join(" + ", allKeys.ToImmutable());
	}

	/// <inheritdoc />
	public override string ToString() => _allKeysStr;
}
