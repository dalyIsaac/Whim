namespace Whim;

/// <summary>
/// Events for the <see cref="IWindowSector"/>
/// </summary>
public interface IWindowSectorEvents
{
	/// <summary>
	/// Event for when a window is added by the <see cref="IWindowSector"/>.
	/// </summary>
	event EventHandler<WindowAddedEventArgs>? WindowAdded;

	/// <summary>
	/// Event for when a window is focused.
	/// </summary>
	event EventHandler<WindowFocusedEventArgs>? WindowFocused;

	/// <summary>
	/// Event for when a window is removed from Whim.
	/// </summary>
	event EventHandler<WindowRemovedEventArgs>? WindowRemoved;

	/// <summary>
	/// Event for when a window is being moved or resized.
	/// </summary>
	event EventHandler<WindowMoveStartedEventArgs>? WindowMoveStarted;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_SYSTEM_MOVESIZEEND"/> event.
	/// See https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants for more information.
	/// </summary>
	event EventHandler<WindowMoveEndedEventArgs>? WindowMoveEnded;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_OBJECT_LOCATIONCHANGE"/> event.
	/// </summary>
	event EventHandler<WindowMovedEventArgs>? WindowMoved;

	/// <summary>
	/// Event for when a window has started being minimized.
	/// </summary>
	event EventHandler<WindowMinimizeStartedEventArgs>? WindowMinimizeStarted;

	/// <summary>
	/// Event for when a window has ended being minimized.
	/// </summary>
	event EventHandler<WindowMinimizeEndedEventArgs>? WindowMinimizeEnded;
}
