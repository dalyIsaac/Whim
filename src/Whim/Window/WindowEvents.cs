using System;

namespace Whim;

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has had something happen to it.
/// </summary>
public class WindowEventArgs : EventArgs
{
	/// <summary>
	/// The <see cref="IWindow"/> that had something happen to it.
	/// </summary>
	public IWindow Window { get; private set; }

	/// <summary>
	/// Create a new <see cref="WindowEventArgs"/>.
	/// </summary>
	/// <param name="window"></param>
	public WindowEventArgs(IWindow window)
	{
		Window = window;
	}
}

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has been updated by Windows.
/// </summary>
public class WindowUpdateEventArgs : WindowEventArgs
{
	/// <summary>
	/// The type of update that happened to the <see cref="IWindow"/>.
	/// </summary>
	public WindowUpdateType UpdateType { get; private set; }

	/// <summary>
	/// Create a new <see cref="WindowUpdateEventArgs"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="updateType"></param>
	public WindowUpdateEventArgs(IWindow window, WindowUpdateType updateType)
		: base(window)
	{
		UpdateType = updateType;
	}
}
