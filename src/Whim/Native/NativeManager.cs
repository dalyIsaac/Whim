using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Manager for interacting with native Windows APIs.
/// </summary>
public class NativeManager : INativeManager
{
	private readonly IConfigContext _configContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeManager"/> class.
	/// </summary>
	/// <param name="configContext"></param>
	public NativeManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	private const int _bufferCapacity = 255;

	/// <inheritdoc />
	public static void QuitWindow(IWindow window)
	{
		Logger.Debug($"Quitting window {window} with handle {window.Handle.Value}");
		PInvoke.SendNotifyMessage(window.Handle, PInvoke.WM_SYSCOMMAND, new WPARAM(PInvoke.SC_CLOSE), 0);
	}

	/// <inheritdoc />
	public static void ForceForegroundWindow(IWindow window)
	{
		Logger.Debug($"Forcing window {window} with handle {window.Handle.Value} to foreground");
		// Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
		// keybd_event synthesizes a keystroke - see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
		PInvoke.keybd_event(0, 0, 0, 0);
		PInvoke.SetForegroundWindow(window.Handle);
	}

	/// <inheritdoc/>
	public static bool HideWindow(IWindow window)
	{
		Logger.Debug($"Hiding window {window} with handle {window.Handle.Value}");
		return (bool)PInvoke.ShowWindow(window.Handle, SHOW_WINDOW_CMD.SW_HIDE);
	}

	/// <inheritdoc />
	public static bool ShowWindowMaximized(IWindow window)
	{
		Logger.Debug($"Showing window {window} with handle {window.Handle.Value} maximized");
		return (bool)PInvoke.ShowWindow(window.Handle, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
	}

	/// <inheritdoc />
	public static bool ShowWindowMinimized(IWindow window)
	{
		Logger.Debug($"Showing window {window} with handle {window.Handle.Value} minimized");
		return (bool)PInvoke.ShowWindow(window.Handle, SHOW_WINDOW_CMD.SW_SHOWMINIMIZED);
	}

	/// <inheritdoc />
	public static bool MinimizeWindow(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} with handle {window.Handle.Value}");
		return (bool)PInvoke.ShowWindow(window.Handle, SHOW_WINDOW_CMD.SW_MINIMIZE);
	}

	/// <inheritdoc />
	public static bool ShowWindowNoActivate(IWindow window)
	{
		Logger.Debug($"Showing window {window} with handle {window.Handle.Value} no activate");
		return (bool)PInvoke.ShowWindow(window.Handle, SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
	}

	/// <inheritdoc />
	public static UnhookWinEventSafeHandle SetWindowsEventHook(
		uint eventMin,
		uint eventMax,
		WINEVENTPROC lpfnWinEventProc
	) => PInvoke.SetWinEventHook(eventMin, eventMax, null, lpfnWinEventProc, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);

	/// <inheritdoc />
	public static string GetClassName(IWindow window)
	{
		unsafe
		{
			fixed (char* buffer = new char[_bufferCapacity])
			{
				int length = PInvoke.GetClassName(window.Handle, buffer, _bufferCapacity + 1);
				return length > 0 ? new string(buffer) : "";
			}
		}
	}

	/// <inheritdoc />
	public static string GetWindowText(IWindow window)
	{
		unsafe
		{
			fixed (char* buffer = new char[_bufferCapacity])
			{
				int length = PInvoke.GetWindowText(window.Handle, buffer, _bufferCapacity + 1);
				return length > 0 ? new string(buffer) : "";
			}
		}
	}

	private const string _splashClassName = "MsoSplash";

	/// <inheritdoc />
	public static bool IsSplashScreen(IWindow window)
	{
		string classname = GetClassName(window);
		if (classname.Length == 0)
		{
			return false;
		}

		return classname == _splashClassName;
	}

	/// <inheritdoc />
	public static bool IsStandardWindow(IWindow window)
	{
		if (
			PInvoke.GetAncestor(window.Handle, GET_ANCESTOR_FLAGS.GA_ROOT) != window.Handle.Value
			|| !PInvoke.IsWindowVisible(window.Handle)
		)
		{
			return false;
		}

		uint style = (uint)PInvoke.GetWindowLong(window.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
		uint exStyle = (uint)PInvoke.GetWindowLong(window.Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

		// WS_POPUP need to have a border or minimize/maximize buttons,
		// otherwise the window is "not interesting"
		if (
			(style & (uint)WINDOW_STYLE.WS_POPUP) == (uint)WINDOW_STYLE.WS_POPUP
			&& (style & (uint)WINDOW_STYLE.WS_THICKFRAME) == 0
			&& (style & (uint)WINDOW_STYLE.WS_MINIMIZEBOX) == 0
			&& (style & (uint)WINDOW_STYLE.WS_MAXIMIZEBOX) == 0
		)
		{
			return false;
		}
		if (
			(style & (uint)WINDOW_STYLE.WS_CHILD) == (uint)WINDOW_STYLE.WS_CHILD
			|| (style & (uint)WINDOW_STYLE.WS_DISABLED) == (uint)WINDOW_STYLE.WS_DISABLED
			|| (exStyle & (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) == (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW
			|| (exStyle & (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE) == (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE
		)
		{
			return false;
		}

		string className = GetClassName(window);
		if (IsSystemWindow(window, className))
		{
			return false;
		}

		return true;
	}

	/// <inheritdoc />
	public static void HideCaptionButtons(IWindow window)
	{
		int style = PInvoke.GetWindowLong(window.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

		// Hide the title bar and caption buttons
		style &= ~(int)WINDOW_STYLE.WS_CAPTION & ~(int)WINDOW_STYLE.WS_THICKFRAME;

		_ = PInvoke.SetWindowLong(window.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);
	}

	private static readonly HashSet<string> _systemClasses =
		new() { "SysListView32", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Progman" };

	/// <inheritdoc />
	public static bool IsSystemWindow(IWindow window, string className)
	{
		if (window.Handle.Value == PInvoke.GetDesktopWindow() || window.Handle.Value == PInvoke.GetShellWindow())
		{
			return true;
		}

		if (_systemClasses.Contains(className))
		{
			return true;
		}

		return false;
	}

	/// <inheritdoc />
	public static bool HasNoVisibleOwner(IWindow window)
	{
		HWND owner = PInvoke.GetWindow(window.Handle, GET_WINDOW_CMD.GW_OWNER);

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

	/// <inheritdoc />
	public static bool IsCloakedWindow(IWindow window)
	{
		unsafe
		{
			int cloaked;
			HRESULT res = PInvoke.DwmGetWindowAttribute(
				window.Handle,
				DWMWINDOWATTRIBUTE.DWMWA_CLOAKED,
				&cloaked,
				sizeof(int)
			);
			return cloaked != 0 || res.Failed;
		}
	}

	/// <inheritdoc />
	public static ILocation<int> GetWindowOffset(IWindow window)
	{
		if (!PInvoke.GetWindowRect(window.Handle, out RECT windowRect))
		{
			Logger.Error($"Could not get the window rect for {window.Handle.Value}");
			return new Location<int>();
		}
		unsafe
		{
			ILocation<int>? extendedFrameLocation = DwmGetWindowLocation(window);
			if (extendedFrameLocation == null)
			{
				return new Location<int>();
			}

			return new Location<int>()
			{
				X = windowRect.left - extendedFrameLocation.X,
				Y = windowRect.top - extendedFrameLocation.Y,
				Width = windowRect.right - windowRect.left - extendedFrameLocation.Width,
				Height = windowRect.bottom - windowRect.top - extendedFrameLocation.Height
			};
		}
	}

	/// <inheritdoc />
	public static ILocation<int>? DwmGetWindowLocation(IWindow window)
	{
		unsafe
		{
			RECT extendedFrameRect = new();
			uint size = (uint)Marshal.SizeOf<RECT>();
			HRESULT res = PInvoke.DwmGetWindowAttribute(
				window.Handle,
				DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
				&extendedFrameRect,
				size
			);

			if (res.Failed)
			{
				Logger.Error($"Could not get the extended frame rect for {window.Handle.Value}");
				return null;
			}

			return new Location<int>()
			{
				X = extendedFrameRect.left,
				Y = extendedFrameRect.top,
				Width = extendedFrameRect.right - extendedFrameRect.left,
				Height = extendedFrameRect.bottom - extendedFrameRect.top
			};
		}
	}

	/// <inheritdoc />
	public static void SetWindowCorners(
		IWindow window,
		DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
	)
	{
		unsafe
		{
			HRESULT res = PInvoke.DwmSetWindowAttribute(
				window.Handle,
				DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
				&preference,
				sizeof(DWM_WINDOW_CORNER_PREFERENCE)
			);
			if (res.Failed)
			{
				Logger.Error($"Failed to set window corners for {window.Handle.Value}");
			}
		}
	}

	/// <inheritdoc />
	public static IEnumerable<HWND> GetAllWindows()
	{
		List<HWND> windows = new();

		PInvoke.EnumWindows(
			(handle, param) =>
			{
				windows.Add(handle);
				return (BOOL)true;
			},
			0
		);

		return windows;
	}
}
