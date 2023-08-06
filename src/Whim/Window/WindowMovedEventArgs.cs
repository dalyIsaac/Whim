using System;

namespace Whim;

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has been moved.
/// </summary>
public class WindowMovedEventArgs : EventArgs
{
	/// <summary>
	/// The <see cref="IWindow"/> that had something happen to it.
	/// </summary>
	public required IWindow Window { get; init; }

	/// <summary>
	/// The cursor point. Only set if the window is being dragged.
	/// </summary>
	public required IPoint<int>? CursorDraggedPoint { get; init; }
}
