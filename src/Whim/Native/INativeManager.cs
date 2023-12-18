using System.Collections.Generic;
using Microsoft.UI.Dispatching;
using Windows.UI.Composition;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;

namespace Whim;

/// <summary>
/// Manager for interacting with native Windows APIs.
/// </summary>
public interface INativeManager
{
	/// <summary>
	/// Quit the window.
	/// </summary>
	/// <param name="hwnd"></param>
	void QuitWindow(HWND hwnd);

	/// <summary>
	/// Force the window to the foreground.
	/// </summary>
	/// <param name="hwnd"></param>
	void ForceForegroundWindow(HWND hwnd);

	/// <summary>
	/// Hides the window of the associated handle.
	/// </summary>
	/// <param name="hwnd"></param>
	bool HideWindow(HWND hwnd);

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	bool ShowWindowMaximized(HWND hwnd);

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	bool ShowWindowMinimized(HWND hwnd);

	/// <summary>
	/// Minimizes the specified window and activates the next top-level window in the Z order.
	/// </summary>
	bool MinimizeWindow(HWND hwnd);

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	bool ShowWindowNoActivate(HWND hwnd);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetClassName"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	string GetClassName(HWND hwnd);

	/// <summary>
	/// Hides the caption buttons from the given window.
	/// </summary>
	/// <param name="hwnd"></param>
	void HideCaptionButtons(HWND hwnd);

	/// <summary>
	/// Prevent the window from being activated.
	/// </summary>
	/// <param name="hwnd"></param>
	void PreventWindowActivation(HWND hwnd);

	/// <summary>
	/// Returns the window's offset.<br/>
	/// This is based on the issue raised at https://github.com/workspacer/workspacer/issues/139,
	/// and the associated fix from https://github.com/workspacer/workspacer/pull/146.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	IRectangle<int>? GetWindowOffset(HWND hwnd);

	/// <summary>
	/// Returns the window's rectangle from DWM.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	IRectangle<int>? DwmGetWindowRectangle(HWND hwnd);

	/// <summary>
	/// Sets the preferred window corners for the given <paramref name="hwnd"/>.
	/// By default, the window corners are rounded.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <param name="preference"></param>
	void SetWindowCorners(
		HWND hwnd,
		DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
	);

	/// <summary>
	/// Creates a handle which will facilitates setting the position of multiple windows at once.
	/// </summary>
	/// <returns></returns>
	DeferWindowPosHandle DeferWindowPos();

	/// <summary>
	/// Creates a handle which will facilitates setting the position of multiple windows at once.
	/// </summary>
	/// <param name="windowStates">The initial window states to set.</param>
	/// <returns></returns>
	DeferWindowPosHandle DeferWindowPos(IEnumerable<WindowPosState> windowStates);

	/// <summary>
	/// Retrieves the path to the executable file of the UWP app associated with the given <paramref name="window"/>.
	/// This will only work for UWP apps (see <see cref="IWindow.IsUwp"/>).
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	string? GetUwpAppProcessPath(IWindow window);

	/// <summary>
	/// The compositor for Whim.
	/// </summary>
	Compositor Compositor { get; }

	/// <summary>
	/// Gets whether the system is using dark mode.
	/// </summary>
	/// <returns></returns>
	bool ShouldSystemUseDarkMode();

	/// <summary>
	/// Creates a <see cref="TransparentWindowController"/> for the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	TransparentWindowController CreateTransparentWindowController(Microsoft.UI.Xaml.Window window);

	/// <summary>
	/// Sets the given <paramref name="hwnd"/> to be transparent to mouse clicks.
	/// </summary>
	/// <param name="hwnd"></param>
	void SetWindowExTransparent(HWND hwnd);

	/// <summary>
	/// Removes the transparency from the given <paramref name="hwnd"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	void RemoveWindowExTransparent(HWND hwnd);

	/// <summary>
	/// Adds a task to the <see cref="DispatcherQueue" /> which will be executed on the thread associated
	/// with the <see cref="DispatcherQueue" />.
	/// </summary>
	/// <param name="callback">The task to execute.</param>
	/// <returns><see langword="true" /> indicates that the task was added to the queue; <see langword="false" />, otherwise.</returns>
	bool TryEnqueue(DispatcherQueueHandler callback);

	/// <summary>
	/// Gets the version of Whim.
	/// </summary>
	/// <returns></returns>
	string GetWhimVersion();
}
