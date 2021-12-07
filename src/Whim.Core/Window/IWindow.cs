namespace Whim.Core.Window;

/// <summary>
/// Delegate for the <see cref="IWindowManager.WindowRegistered"/> event.
/// </summary>
/// <param name="window">The newly added <see cref="IWindow"/>.</param>
public delegate void WindowRegisterDelegate(IWindow window);

/// <summary>
/// Delegate for the <see cref="IWindow.WindowFocused"/> event.
/// </summary>
/// <param name="window">The focused <see cref="IWindow"/>.</param>
public delegate void WindowFocusDelegate(IWindow window);

/// <summary>
/// Delegate for the <see cref="IWindow.WindowUpdated"/> event.
/// </summary>
/// <param name="window">The updated <see cref="IWindow"/>.</param>
/// <param name="updateType">A description of the update type.</param>
public delegate void WindowUpdateDelegate(IWindow window, WindowUpdateType updateType);

/// <summary>
/// Delegate for the see <see cref="IWindow.WindowUnregistered"/> event.
/// </summary>
/// <param name="window">The <see cref="IWindow"/> being unregistered.</param>
public delegate void WindowUnregisterDelegate(IWindow window);

/// <summary>
/// Represents a single window.
/// </summary>
public interface IWindow
{
	/// <summary>
	/// The handle of the window.
	/// </summary>
	public int Handle { get;}

	/// <summary>
	/// The title of the window.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// The name of the class to which the window belongs.
	/// </summary>
	public string Class { get; }

	/// <summary>
	/// Retrieves the dimensions of the bounding rectangle of the specified window.
	/// </summary>
	public ILocation Location { get; }

	/// <summary>
	/// The process ID of the window.
	/// </summary>
	public int ProcessId { get; }

	/// <summary>
	/// The fully qualified path that defines the location of the module.
	/// </summary>
	public string ProcessFileName { get; }

	/// <summary>
	/// The name that the system uses to identify the process to the user.
	/// </summary>
	public string ProcessName { get; }

	/// <summary>
	/// Indicates whether the window is focused.
	/// </summary>
	public bool IsFocused { get; }

	/// <summary>
	/// Indiciates whether the window is minimized.
	/// </summary>
	public bool IsMinimized { get; }

	/// <summary>
	/// Indicates whether the window is maximised.
	/// </summary>
	public bool IsMaximized { get; }

	/// <summary>
	/// Indicates whether the mouse is moving the window.
	/// </summary>
	public bool IsMouseMoving { get; }

	/// <summary>
	/// The <see cref="IWindowManager"/> which is managing this window.
	/// </summary>
	public IWindowManager WindowManager { get; }

	/// <summary>
	/// Event for when this window is updated.
	/// </summary>
	public event WindowUpdateDelegate WindowUpdated;

	/// <summary>
	/// Event for when this window is focused.
	/// </summary>
	public event WindowFocusDelegate WindowFocused;

	/// <summary>
	/// Event for when this window is unregistered.
	/// </summary>
	public event WindowUnregisterDelegate WindowUnregistered;

	/// <summary>
	/// Moves the focus to this window.
	/// </summary>
	public void Focus();

	/// <summary>
	/// Hides this window.
	/// </summary>
	public void Hide();

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	public void ShowNormal();

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	public void ShowMaximized();

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	public void ShowMinimized();

	/// <summary>
	/// Shows the window in the current state. <br/>
	///
	/// If the window is minimized, the window is shown minimized
	/// (see <see cref="ShowMinimized"/>)<br/>
	///
	/// If the window is maximized, the window is shown maximized
	/// (see <see cref="ShowMaximized"/>). <br/>
	///
	/// Otherwise, the window is shown in its most recent size and position
	/// (see <see cref="ShowNormal"/>).
	/// </summary>
	public void ShowInCurrentState();

	/// <summary>
	/// Brings the window to the top.
	/// </summary>
	public void BringToTop();

	/// <summary>
	/// Quits the window.
	/// </summary>
	public void Close();

	/// <summary>
	/// Handles the events raised by the <see cref="WindowManager"/> from Windows.
	/// </summary>
	/// <param name="eventType">The window type.</param>
	internal void HandleEvent(uint eventType);
}
