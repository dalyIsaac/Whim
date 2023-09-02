namespace Whim;

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has been moved.
/// </summary>
public class WindowMovedEventArgs : WindowEventArgs
{
	/// <summary>
	/// The cursor point. Only set if the window is being dragged.
	/// </summary>
	public required IPoint<int>? CursorDraggedPoint { get; init; }

	/// <summary>
	/// The edges that were moved.
	/// </summary>
	public required Direction? MovedEdges { get; init; }
}
