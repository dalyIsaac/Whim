using System.Collections.Generic;
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

	private const string _splashClassName = "MsoSplash";

	/// <summary>
	/// Returns <see langword="true"/> if the window is a splash window.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public static bool IsSplashScreen(HWND hwnd)
	{
		string classname = GetClassName(hwnd);
		if (classname.Length == 0)
		{
			return false;
		}

		return classname == _splashClassName;
	}

	/// <summary>
	/// Returns <see langword="true"/> if the window is a standard window.
	/// Based on https://github.com/microsoft/PowerToys/blob/fa81968dbb58a0697c45335a8f453e5794852348/src/modules/fancyzones/FancyZonesLib/FancyZones.cpp#L381
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public static bool IsStandardWindow(HWND hwnd)
	{
		if (PInvoke.GetAncestor(hwnd, GET_ANCESTOR_FLAGS.GA_ROOT) != hwnd || !PInvoke.IsWindowVisible(hwnd))
		{
			return false;
		}

		uint style = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
		uint exStyle = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

		// WS_POPUP need to have a border or minimize/maximize buttons,
		// otherwise the window is "not interesting"
		if ((style & (uint)WINDOW_STYLE.WS_POPUP) == (uint)WINDOW_STYLE.WS_POPUP &&
		   (style & (uint)WINDOW_STYLE.WS_THICKFRAME) == 0 &&
		   (style & (uint)WINDOW_STYLE.WS_MINIMIZEBOX) == 0 &&
		   (style & (uint)WINDOW_STYLE.WS_MAXIMIZEBOX) == 0)
		{
			return false;
		}
		if ((style & (uint)WINDOW_STYLE.WS_CHILD) == (uint)WINDOW_STYLE.WS_CHILD ||
			(style & (uint)WINDOW_STYLE.WS_DISABLED) == (uint)WINDOW_STYLE.WS_DISABLED ||
			(exStyle & (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) == (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW ||
			(exStyle & (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE) == (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE)
		{
			return false;
		}

		string className = GetClassName(hwnd);
		if (IsSystemWindow(hwnd, className))
		{
			return false;
		}

		return true;
	}

	private static readonly HashSet<string> _systemClasses = new() { "SysListView32", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Progman" };

	/// <summary>
	/// Returns <see langword="true"/> if the window is a system window.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <param name="className">The window's class name.</param>
	/// <returns></returns>
	public static bool IsSystemWindow(HWND hwnd, string className)
	{
		if (hwnd == PInvoke.GetDesktopWindow() || hwnd == PInvoke.GetShellWindow())
		{
			return true;
		}

		if (_systemClasses.Contains(className))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Returns <see langword="true"/> when the window has no visible owner.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public static bool HasNoVisibleOwner(HWND hwnd)
	{
		HWND owner = PInvoke.GetWindow(hwnd, GET_WINDOW_CMD.GW_OWNER);

		// The following warning was disabled, since GetWindow can return null, per
		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindow
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
		if (owner == null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
		{
			return true; // There is no owner at all
		}

		if (!PInvoke.IsWindowVisible(owner))
		{
			return true; // Owner is invisible
		}

		if (!PInvoke.GetWindowRect(owner, out RECT rect))
		{
			return false; // Could not get the rect, return true (and filter out the window) just in case
		}

		// It is enough that the window is zero-sized in one dimension only.
		return rect.top == rect.bottom || rect.left == rect.right;
	}

	/// <summary>
	/// Returns <see langword="true"/> when the window is a cloaked window.
	/// For example, and empty <c>ApplicationFrameWindow</c>.
	/// For more, see https://social.msdn.microsoft.com/Forums/vstudio/en-US/f8341376-6015-4796-8273-31e0be91da62/difference-between-actually-visible-and-not-visiblewhich-are-there-but-we-cant-see-windows-of?forum=vcgeneral
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public static bool IsCloakedWindow(HWND hwnd)
	{
		unsafe
		{
			int cloaked;
			HRESULT res = PInvoke.DwmGetWindowAttribute(hwnd,
						Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED,
						&cloaked,
						sizeof(int));
			return cloaked != 0 || res.Failed;
		}
	}

	private static readonly HWND HWND_TOP = new(0);

	/// <summary>
	/// Using the given <see paramref="loc"/>, sets the window's position.
	/// </summary>
	/// <param name="loc"></param>
	public static void SetWindowPos(IWindowLocation windowLocation)
	{

		IWindow window = windowLocation.Window;
		ILocation location = windowLocation.Location;
		WindowState windowState = windowLocation.WindowState;

		SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
							   | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
							   | SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
							   | SET_WINDOW_POS_FLAGS.SWP_NOZORDER
							   | SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

		if (windowState == WindowState.Maximized || windowState == WindowState.Minimized)
		{
			flags = flags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}

		// We use HWND_BOTTOM, as modifying the Z-order of a window
		// may cause EVENT_SYSTEM_FOREGROUND to be set to Whim, which in turn
		// causes the relevant window to be focused, when the user hasn't
		// actually changed the focus.

		PInvoke.SetWindowPos(
			window.Handle,
			(HWND)(1), // HWND_BOTTOM
			location.X,
			location.Y,
			location.Width,
			location.Height,
			flags);

		if (windowState == WindowState.Maximized)
		{
			if (!window.IsMinimized)
			{
				MinimizeWindow(window.Handle);
			}
		}
		else if (windowState == WindowState.Minimized)
		{
			if (!window.IsMaximized)
			{
				ShowWindowMaximized(window.Handle);
			}
		}
		else if (window.Class != "Windows.UI.Core.CoreWindow")
		{
			ShowWindowNoActivate(window.Handle);
		}
	}
}
