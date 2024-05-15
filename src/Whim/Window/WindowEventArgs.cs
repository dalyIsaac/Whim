using System;

namespace Whim;

/// <summary>
/// Base event arguments for when a <see cref="IWindow"/> has had something happen to it.
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
/// Event arguments for when a <see cref="IWindow"/> has been removed from Whim and Windows.
/// </summary>
public class WindowRemovedEventArgs : WindowEventArgs { }

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has been minimized.
/// </summary>
public class WindowMinimizeStartedEventArgs : WindowEventArgs { }

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> is no longer minimized.
/// </summary>
public class WindowMinimizeEndedEventArgs : WindowEventArgs { }

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

/// <summary>
/// Base event arguments for when a <see cref="IWindow"/> has been moved.
/// </summary>
public abstract class WindowMoveEventArgs : WindowEventArgs
{
	/// <summary>
	/// The cursor point. Only set if the window is being dragged.
	/// </summary>
	public required IPoint<int>? CursorDraggedPoint { get; init; }

	/// <summary>
	/// The edges that were moved, if the window was being resized by dragging the edges.
	/// </summary>
	public required Direction? MovedEdges { get; init; }
}

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has started moving.
/// </summary>
public class WindowMoveStartedEventArgs : WindowMoveEventArgs { }

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has stopped moving.
/// </summary>
public class WindowMoveEndedEventArgs : WindowMoveEventArgs { }

/// <summary>
/// Event arguments for when a window has changed location, shape, or size.
/// </summary>
public class WindowMovedEventArgs : WindowMoveEventArgs { }
