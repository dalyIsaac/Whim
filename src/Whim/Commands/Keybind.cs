using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

public class Keybind : IKeybind
{
	public KeyModifiers Modifiers { get; }
	public VIRTUAL_KEY Key { get; }

	/// <summary>
	/// The keys which make up this keybind.
	/// </summary>
	public ReadOnlyCollection<string> AllKeys { get; }

	/// <summary>
	/// Saved representation of the keybind as a string.
	/// </summary>
	private readonly string _allKeysStr;

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

	public override int GetHashCode() => HashCode.Combine(Modifiers, Key);

	public override string ToString() => _allKeysStr;
}
