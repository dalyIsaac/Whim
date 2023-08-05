using System;

namespace Whim;

/// <summary>
/// Event arguments for mouse events.
/// </summary>
/// <param name="Point">The x- and y-coordinates of the cursor, in per-monitor-aware screen coordinates.</param>
internal record MouseEventArgs(IPoint<int> Point);

internal interface IMouseHook
{
	/// <summary>
	/// Event that is fired when the mouse left button is pressed down.
	/// </summary>
	event EventHandler<MouseEventArgs>? MouseLeftButtonDown;
}
