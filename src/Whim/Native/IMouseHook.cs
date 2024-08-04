namespace Whim;

/// <summary>
/// Event arguments for mouse events.
/// </summary>
internal class MouseEventArgs(IPoint<int> point) : EventArgs
{
	public IPoint<int> Point { get; } = point;
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

	/// <summary>
	/// Initializes the mouse hook.
	/// </summary>
	void PostInitialize();
}
