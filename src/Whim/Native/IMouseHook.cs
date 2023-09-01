using System;

namespace Whim;

/// <summary>
/// Event arguments for mouse events.
/// </summary>
internal class MouseEventArgs : EventArgs
{
	public IPoint<int> Point { get; }

	public MouseEventArgs(IPoint<int> point)
	{
		Point = point;
	}
}

internal interface IMouseHook : IDisposable
{
	/// <summary>
	/// Event that is fired when the mouse left button is pressed down.
	/// </summary>
	event EventHandler<MouseEventArgs>? MouseLeftButtonDown;

	/// <summary>
	/// Event that is fired when the mouse left button is released.
	/// </summary>
	event EventHandler<MouseEventArgs>? MouseLeftButtonUp;
}
