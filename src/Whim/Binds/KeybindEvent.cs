using System;

namespace Whim;

public class KeybindEventArgs : EventArgs
{
	public Keybind Keybind { get; }

	public KeybindEventArgs(Keybind keybind)
	{
		Keybind = keybind;
	}
}
