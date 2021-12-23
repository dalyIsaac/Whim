using System;

namespace Whim;

public delegate void KeybindEventHandler(KeybindEventArgs args);

public class KeybindEventArgs : EventArgs
{
	public Keybind Keybind { get; }

	public KeybindEventArgs(Keybind keybind)
	{
		Keybind = keybind;
	}
}
