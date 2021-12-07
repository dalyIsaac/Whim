using System;

namespace Whim.Core.Window;

/// <summary>
/// Delegate for the <see cref="IWindowManager.WindowRegistered"/> event.
/// </summary>
/// <param name="sender">The sender of the event.</param>
/// <param name="args">
/// The <see cref="WindowEventArgs"/> containing the newly added <see cref="IWindow"/>.
/// </param>
public delegate void WindowRegisterDelegate(object sender, WindowEventArgs args);

/// <summary>
/// Delegate for the <see cref="IWindow.WindowFocused"/> event.
/// </summary>
/// <param name="sender">The sender of the event.</param>
/// <param name="args">
/// The <see cref="WindowEventArgs"/> containing the focused <see cref="IWindow"/>.
/// </param>
public delegate void WindowFocusDelegate(object sender, WindowEventArgs args);

/// <summary>
/// Delegate for the <see cref="IWindow.WindowUpdated"/> event.
/// </summary>
/// <param name="sender">The sender of the event.</param>
/// <param name="args">
/// The <see cref="WindowEventArgs"/> containing the updated <see cref="IWindow"/>, and the
/// description of the update type.
/// </param>
public delegate void WindowUpdateDelegate(object sender, WindowUpdateEventArgs args);

/// <summary>
/// Delegate for the see <see cref="IWindow.WindowUnregistered"/> event.
/// </summary>
/// <param name="sender">The sender of the event.</param>
/// <param name="args">
/// The <see cref="WindowEventArgs"/> containing the <see cref="IWindow"/> being unregistered.
/// </param>
public delegate void WindowUnregisterDelegate(object sender, WindowEventArgs args);

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
