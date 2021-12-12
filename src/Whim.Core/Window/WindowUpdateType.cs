namespace Whim.Core.Window;

/// <summary>
/// The window update type. Based on event constants from
/// https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants
/// </summary>
public enum WindowUpdateType
{
	/// <summary>
	/// Sent when a window is uncloaked. A cloaked window still exists, but is
	/// invisible to the user.
	/// For cloaking, see https://devblogs.microsoft.com/oldnewthing/20200302-00/?p=103507
	/// </summary>
	Uncloaked,

	/// <summary>
	/// Sent when a window is cloaked. A cloaked window still exists, but is
	/// invisible to the user.
	/// For cloaking, see https://devblogs.microsoft.com/oldnewthing/20200302-00/?p=103507
	/// </summary>
	Cloaked,

	/// <summary>
	/// A window object is about to be minimized.
	/// </summary>
	MinimizeStart,

	/// <summary>
	/// A window object is about to be restored.
	/// </summary>
	MinimizeEnd,

	/// <summary>
	/// The foreground window has changed. The system sends this event even if
	/// the foreground window has changed to another window in the same thread.
	/// </summary>
	Foreground,

	/// <summary>
	/// A window is being moved or resized.
	/// </summary>
	MoveStart,

	/// <summary>
	/// The movement or resizing of a window has finished.
	/// </summary>
	MoveEnd,

	/// <summary>
	/// An object has changed location, shape, or size. The system sends this
	/// event for the following user interface elements: caret and window
	/// objects.
	/// This event is generated in response to a change in the top-level object
	/// within the object hierarchy; it is not generated for any children that
	/// the object might have.
	/// </summary>
	Move,
}
