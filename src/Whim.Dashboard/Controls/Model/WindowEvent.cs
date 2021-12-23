using System;

namespace Whim.Dashboard.Controls.Model;

internal class WindowEventArgs : EventArgs
{
	internal Window Window { get; }

	internal WindowEventArgs(Window window)
	{
		Window = window;
	}
}
