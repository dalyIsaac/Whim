using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;

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
	public void QuitWindow(HWND hwnd);

	/// <summary>
	/// Force the window to the foreground.
	/// </summary>
	/// <param name="hwnd"></param>
	public void ForceForegroundWindow(HWND hwnd);

	/// <summary>
	/// Hides the window of the associated handle.
	/// </summary>
	/// <param name="hwnd"></param>
	public bool HideWindow(HWND hwnd);

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	public bool ShowWindowMaximized(HWND hwnd);

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	public bool ShowWindowMinimized(HWND hwnd);

	/// <summary>
	/// Minimizes the specified window and activates the next top-level window in the Z order.
	/// </summary>
	public bool MinimizeWindow(HWND hwnd);

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	public bool ShowWindowNoActivate(HWND hwnd);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetClassName"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public string GetClassName(HWND hwnd);

	/// <summary>
	/// Hides the caption buttons from the given window.
	/// </summary>
	/// <param name="hwnd"></param>
	public void HideCaptionButtons(HWND hwnd);

	/// <summary>
	/// Prevent the window from being activated.
	/// </summary>
	/// <param name="hwnd"></param>
	public void PreventWindowActivation(HWND hwnd);

	/// <summary>
	/// Returns the window's offset.<br/>
	/// This is based on the issue raised at https://github.com/workspacer/workspacer/issues/139,
	/// and the associated fix from https://github.com/workspacer/workspacer/pull/146.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public ILocation<int> GetWindowOffset(HWND hwnd);

	/// <summary>
	/// Returns the window's location from DWM.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public ILocation<int>? DwmGetWindowLocation(HWND hwnd);

	/// <summary>
	/// Sets the preferred window corners for the given <paramref name="hwnd"/>.
	/// By default, the window corners are rounded.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <param name="preference"></param>
	public void SetWindowCorners(
		HWND hwnd,
		DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
	);

	/// <summary>
	/// Allocates memory for a multiple-window position structure and returns the handle to the structure.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.BeginDeferWindowPos(int)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-begindeferwindowpos
	/// </remarks>
	/// <param name="nNumWindows"></param>
	/// <returns></returns>
	public HDWP BeginDeferWindowPos(int nNumWindows);

	/// <summary>
	/// Updates the specified multiple-window position structure for the specified window.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.DeferWindowPos(HDWP, HWND, HWND, int, int, int, int, SET_WINDOW_POS_FLAGS)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-deferwindowpos
	/// </remarks>
	/// <param name="hWinPosInfo"></param>
	/// <param name="hWnd"></param>
	/// <param name="hWndInsertAfter"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="cx"></param>
	/// <param name="cy"></param>
	/// <param name="uFlags"></param>
	/// <returns></returns>
	public HDWP DeferWindowPos(
		HDWP hWinPosInfo,
		HWND hWnd,
		HWND hWndInsertAfter,
		int x,
		int y,
		int cx,
		int cy,
		SET_WINDOW_POS_FLAGS uFlags
	);

	/// <summary>
	/// Simultaneously updates the position, size, shape, content, and translucency of the specified windows in a single-refreshing cycle.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.EndDeferWindowPos(HDWP)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-enddeferwindowpos
	/// </remarks>
	/// <param name="hWinPosInfo"></param>
	/// <returns></returns>
	public bool EndDeferWindowPos(HDWP hWinPosInfo);
}
