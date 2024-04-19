using System;

namespace Whim;

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has had something happen to it.
/// </summary>
public abstract class WindowEventArgs : EventArgs
{
	/// <summary>
	/// The <see cref="IWindow"/> that had something happen to it.
	/// </summary>
	public required IWindow Window { get; init; }
}

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has been added to Whim and Windows.
/// </summary>
public class WindowAddedEventArgs : WindowEventArgs { }

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has been added to Whim and Windows.
/// </summary>
public class WindowRemovedEventArgs : WindowEventArgs { }

/// <summary>
/// Event arguments for when the focused window has changed. When <see cref="Window"/> is
/// <see langword="null"/>, then Whim does not track the focused window.
/// </summary>
public class WindowFocusedEventArgs : EventArgs
{
	/// <summary>
	/// The <see cref="IWindow"/> that is now focused. If <see langword="null"/>, then Whim does not
	/// track the focused window.
	/// </summary>
	public IWindow? Window { get; init; }
}
