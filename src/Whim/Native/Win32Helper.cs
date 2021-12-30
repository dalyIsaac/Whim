using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Accessibility;

namespace Whim;

public static class Win32Helper
{
	private const int _bufferCapacity = 255;

	/// <summary>
	/// Quit the window.
	/// </summary>
	/// <param name="hwnd"></param>
	public static void QuitApplication(HWND hwnd)
	{
		PInvoke.SendNotifyMessage(hwnd, PInvoke.WM_SYSCOMMAND, new WPARAM(PInvoke.SC_CLOSE), 0);
	}

	/// <summary>
	/// Force the window to the foreground.
	/// </summary>
	/// <param name="hwnd"></param>
	public static void ForceForegroundWindow(HWND hwnd)
	{
		// Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
		// keybd_event synthesizes a keystroke - see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
		PInvoke.keybd_event(0, 0, 0, 0);
		PInvoke.SetForegroundWindow(hwnd);
	}

	/// <summary>
	/// Hides the window of the associated handle.
	/// </summary>
	/// <param name="hwnd"></param>
	public static void HideWindow(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_HIDE);
	}

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	public static void ShowWindowMaximized(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
	}

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	public static void ShowWindowMinimized(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMINIMIZED);
	}

	/// <summary>
	/// Minimizes the specified window and activates the next top-level window in the Z order.
	/// </summary>
	public static void MinimizeWindow(HWND hwnd)
	{
		PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_MINIMIZE);
	}

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	public static void ShowWindowNoActivate(HWND hwnd)
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
	public static UnhookWinEventSafeHandle SetWindowsEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc)
		=> PInvoke.SetWinEventHook(eventMin, eventMax, null, lpfnWinEventProc, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetClassName"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public static string GetClassName(HWND hwnd)
	{
		unsafe
		{
			fixed (char* buffer = new char[_bufferCapacity])
			{
				int length = PInvoke.GetClassName(hwnd, buffer, _bufferCapacity + 1);
				return length > 0 ? new string(buffer) : "";
			}
		}
	}

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetWindowText"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public static string GetWindowText(HWND hwnd)
	{
		unsafe
		{
			fixed (char* buffer = new char[_bufferCapacity])
			{
				int length = PInvoke.GetWindowText(hwnd, buffer, _bufferCapacity + 1);
				return length > 0 ? new string(buffer) : "";
			}
		}
	}
}
