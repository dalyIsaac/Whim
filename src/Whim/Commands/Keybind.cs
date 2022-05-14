using System;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

public class Keybind : IKeybind
{
	public KeyModifiers Modifiers { get; }
	public VIRTUAL_KEY Key { get; }

	public Keybind(KeyModifiers modifiers, VIRTUAL_KEY key)
	{
		Modifiers = modifiers;
		Key = key;
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

	public override string ToString() => $"{Modifiers} + {Key}";
}
