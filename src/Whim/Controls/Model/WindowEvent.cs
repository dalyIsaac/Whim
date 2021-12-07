using System;

namespace Whim.Controls.Model;

internal delegate void WindowUnregisteredDelegate(object sender, WindowEventArgs args);

internal class WindowEventArgs : EventArgs
{
	internal Window Window { get; }

	internal WindowEventArgs(Window window)
	{
		Window = window;
	}
}
