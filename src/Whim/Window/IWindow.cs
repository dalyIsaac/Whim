using System;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Represents a single window.
/// </summary>
public interface IWindow
{
	/// <summary>
	/// The handle of the window.
	/// </summary>
	public HWND Handle { get; }

	/// <summary>
	/// The title of the window.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// The name of the class to which the window belongs.
	/// </summary>
	public string WindowClass { get; }

	/// <summary>
	/// The location of the window.
	/// </summary>
	public ILocation<int> Location { get; }

	/// <summary>
	/// The center of the window.
	/// </summary>
	public IPoint<int> Center { get; }

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
	public bool IsMouseMoving { get; set; }

	/// <summary>
	/// Moves the focus to this window.
	/// </summary>
	public void Focus();

	/// <summary>
	/// Forces the window to the foreground and to be focused.
	/// </summary>
	public void FocusForceForeground();

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
	/// Create a new window. If the window cannot be created, <c>null</c> is returned.
	/// </summary>
	/// <param name="hwnd">The handle of the window.</param>
	/// <param name="configContext">The configuration context.</param>
	public static IWindow? CreateWindow(HWND hwnd, IConfigContext configContext)
	{
		Logger.Debug($"Adding window {hwnd.Value}");

		try
		{
			return new Window(hwnd, configContext);
		}
		catch (Exception e)
		{
			Logger.Error($"Could not create a Window instance for {hwnd.Value}, {e.Message}");
			return null;
		}
	}
}
