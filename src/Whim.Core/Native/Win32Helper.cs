using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Accessibility;

namespace Whim.Core.Native;

// This is scoped to internal until https://github.com/microsoft/CsWin32/issues/213 is resolved
internal static class Win32Helper
{
	/// <summary>
	/// Quit the window.
	/// </summary>
	/// <param name="hwnd"></param>
	internal static void QuitApplication(HWND hwnd)
	{
		PInvoke.SendNotifyMessage(hwnd, PInvoke.WM_SYSCOMMAND, new WPARAM(PInvoke.SC_CLOSE), 0);
	}

	/// <summary>
	/// Force the window to the foreground.
	/// </summary>
	/// <param name="hwnd"></param>
	internal static void ForceForegroundWindow(HWND hwnd)
	{
		// Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
		PInvoke.keybd_event(0, 0, 0, 0);
		PInvoke.SetForegroundWindow(hwnd);
	}

	/// <summary>
	/// Hides the window of the associated handle.
	/// </summary>
	/// <param name="hwnd"></param>
	internal static void HideWindow(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_HIDE);
	}

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	internal static void ShowMaximizedWindow(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
	}

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	internal static void ShowMinimizedWindow(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMINIMIZED);
	}

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	internal static void ShowNormalWindow(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
	}

	/// <summary>
	/// Set the <see cref="PInvoke.SetWinEventHook"/> <br/>.
	///
	/// For more, see https://docs.microsoft.com/en-au/windows/win32/api/winuser/nf-winuser-setwineventhook
	/// </summary>
	/// <param name="eventMin"></param>
	/// <param name="eventMax"></param>
	/// <param name="lpfnWinEventProc"></param>
	/// <returns></returns>
	internal static UnhookWinEventSafeHandle SetWindowsEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc)
		=> PInvoke.SetWinEventHook(eventMin, eventMax, null, lpfnWinEventProc, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);
}
