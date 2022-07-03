using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <inheritdoc />
public class Keybind : IKeybind
{
	/// <inheritdoc />
	public KeyModifiers Modifiers { get; }

	/// <inheritdoc />
	public VIRTUAL_KEY Key { get; }

	/// <summary>
	/// The keys which make up this keybind.
	/// </summary>
	public ReadOnlyCollection<string> AllKeys { get; }

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

		List<string> allKeys = new();
		allKeys.AddRange(Modifiers.GetParts());
		allKeys.Add(Key.GetKeyString());

		AllKeys = allKeys.AsReadOnly();
		_allKeysStr = string.Join(" + ", AllKeys);
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is Keybind keybind &&
			   Modifiers == keybind.Modifiers &&
			   Key == keybind.Key;
	}

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(Modifiers, Key);

	/// <inheritdoc />
	public override string ToString() => _allKeysStr;
}
