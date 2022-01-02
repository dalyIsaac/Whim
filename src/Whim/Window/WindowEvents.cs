using System;

namespace Whim;

public class WindowEventArgs : EventArgs
{
	public IWindow Window { get; private set; }

	public WindowEventArgs(IWindow window)
	{
		Window = window;
	}
}

public class WindowUpdateEventArgs : WindowEventArgs
{
	public WindowUpdateType UpdateType { get; private set; }

	public WindowUpdateEventArgs(IWindow window, WindowUpdateType updateType)
		: base(window)
	{
		UpdateType = updateType;
	}
}
