using System;

namespace Whim.Dashboard.Windows;

internal class WindowEventArgs : EventArgs
{
	internal Window Window { get; }

	internal WindowEventArgs(Window window)
	{
		Window = window;
	}
}
