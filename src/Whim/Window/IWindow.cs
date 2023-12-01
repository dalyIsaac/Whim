using Microsoft.UI.Xaml.Media.Imaging;
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
	HWND Handle { get; }

	/// <summary>
	/// The title of the window.
	/// </summary>
	string Title { get; }

	/// <summary>
	/// The name of the class to which the window belongs.
	/// </summary>
	string WindowClass { get; }

	/// <summary>
	/// Whether we think that the window is a UWP app.
	/// </summary>
	bool IsUwp { get; }

	/// <summary>
	/// The containing rectangle for the window.
	/// </summary>
	IRectangle<int> Rectangle { get; }

	/// <summary>
	/// The center of the window.
	/// </summary>
	IPoint<int> Center { get; }

	/// <summary>
	/// The process ID of the window.
	/// </summary>
	int ProcessId { get; }

	/// <summary>
	/// The file name of the module.
	/// </summary>
	string? ProcessFileName { get; }

	/// <summary>
	/// The fully qualified path that defines the location of the module.
	/// </summary>
	string? ProcessFilePath { get; }

	/// <summary>
	/// The name that the system uses to identify the process to the user.
	/// </summary>
	string? ProcessName { get; }

	/// <summary>
	/// Indicates whether the window is focused.
	/// </summary>
	bool IsFocused { get; }

	/// <summary>
	/// Indiciates whether the window is minimized.
	/// </summary>
	bool IsMinimized { get; }

	/// <summary>
	/// Indicates whether the window is maximised.
	/// </summary>
	bool IsMaximized { get; }

	/// <summary>
	/// Forces the window to the foreground and to be focused.
	/// </summary>
	void Focus();

	/// <summary>
	/// Hides this window.
	/// </summary>
	void Hide();

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	void ShowNormal();

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	void ShowMaximized();

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	void ShowMinimized();

	/// <summary>
	/// Brings the window to the top.
	/// </summary>
	void BringToTop();

	/// <summary>
	/// Quits the window.
	/// </summary>
	void Close();

	/// <summary>
	/// Gets the icon of the window.
	/// </summary>
	/// <returns></returns>
	BitmapImage? GetIcon();
}
