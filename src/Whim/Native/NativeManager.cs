using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void QuitWindow(HWND hwnd)
	{
		Logger.Debug($"Quitting application with HWND {hwnd.Value}");
		PInvoke.SendNotifyMessage(hwnd, PInvoke.WM_SYSCOMMAND, new WPARAM(PInvoke.SC_CLOSE), 0);
	}

	/// <inheritdoc/>
	public void ForceForegroundWindow(HWND hwnd)
	{
		Logger.Debug($"Forcing window HWND {hwnd.Value} to foreground");
		// Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
		// keybd_event synthesizes a keystroke - see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
		PInvoke.keybd_event(0, 0, 0, 0);
		PInvoke.SetForegroundWindow(hwnd);
	}

	/// <inheritdoc/>
	public bool HideWindow(HWND hwnd)
	{
		Logger.Debug($"Hiding window HWND {hwnd.Value}");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_HIDE);
	}

	/// <inheritdoc/>
	public bool ShowWindowMaximized(HWND hwnd)
	{
		Logger.Debug($"Showing window HWND {hwnd.Value} maximized");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
	}

	/// <inheritdoc/>
	public bool ShowWindowMinimized(HWND hwnd)
	{
		Logger.Debug($"Showing window HWND {hwnd.Value} minimized");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMINIMIZED);
	}

	/// <inheritdoc/>
	public bool MinimizeWindow(HWND hwnd)
	{
		Logger.Debug($"Minimizing window HWND {hwnd.Value}");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_MINIMIZE);
	}

	/// <inheritdoc/>
	public bool ShowWindowNoActivate(HWND hwnd)
	{
		Logger.Debug($"Showing window HWND {hwnd.Value} no activate");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
	}

	/// <inheritdoc/>
	public string GetClassName(HWND hwnd)
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

	/// <inheritdoc/>
	public string GetWindowText(HWND hwnd)
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

	/// <inheritdoc/>
	public bool IsSplashScreen(HWND hwnd)
	{
		string classname = GetClassName(hwnd);
		if (classname.Length == 0)
		{
			return false;
		}

		return classname == _splashClassName;
	}

	/// <inheritdoc/>
	public bool IsStandardWindow(HWND hwnd)
	{
		if (PInvoke.GetAncestor(hwnd, GET_ANCESTOR_FLAGS.GA_ROOT) != hwnd || !PInvoke.IsWindowVisible(hwnd))
		{
			return false;
		}

		uint style = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
		uint exStyle = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

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

		string className = GetClassName(hwnd);
		if (IsSystemWindow(hwnd, className))
		{
			return false;
		}

		return true;
	}

	/// <inheritdoc/>
	public void HideCaptionButtons(HWND hwnd)
	{
		int style = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

		// Hide the title bar and caption buttons
		style &= ~(int)WINDOW_STYLE.WS_CAPTION & ~(int)WINDOW_STYLE.WS_THICKFRAME;

		_ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);
	}

	/// <inheritdoc/>
	public void PreventWindowActivation(HWND hwnd)
	{
		int exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

		// Prevent the window from being activated
		exStyle |= (int)WINDOW_EX_STYLE.WS_EX_NOACTIVATE;

		_ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);
	}

	private readonly HashSet<string> _systemClasses =
		new() { "SysListView32", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Progman" };

	/// <inheritdoc/>
	public bool IsSystemWindow(HWND hwnd, string className)
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

	/// <inheritdoc/>
	public bool HasNoVisibleOwner(HWND hwnd)
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

	/// <inheritdoc/>
	public bool IsCloakedWindow(HWND hwnd)
	{
		unsafe
		{
			int cloaked;
			HRESULT res = PInvoke.DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, &cloaked, sizeof(int));
			return cloaked != 0 || res.Failed;
		}
	}

	/// <inheritdoc/>
	public ILocation<int> GetWindowOffset(HWND hwnd)
	{
		if (!PInvoke.GetWindowRect(hwnd, out RECT windowRect))
		{
			Logger.Error($"Could not get the window rect for {hwnd.Value}");
			return new Location<int>();
		}
		unsafe
		{
			ILocation<int>? extendedFrameLocation = DwmGetWindowLocation(hwnd);
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

	/// <inheritdoc/>
	public ILocation<int>? DwmGetWindowLocation(HWND hwnd)
	{
		unsafe
		{
			RECT extendedFrameRect = new();
			uint size = (uint)Marshal.SizeOf<RECT>();
			HRESULT res = PInvoke.DwmGetWindowAttribute(
				hwnd,
				DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
				&extendedFrameRect,
				size
			);

			if (res.Failed)
			{
				Logger.Error($"Could not get the extended frame rect for {hwnd.Value}");
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

	/// <inheritdoc/>
	public void SetWindowCorners(
		HWND hwnd,
		DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
	)
	{
		unsafe
		{
			HRESULT res = PInvoke.DwmSetWindowAttribute(
				hwnd,
				DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
				&preference,
				sizeof(DWM_WINDOW_CORNER_PREFERENCE)
			);
			if (res.Failed)
			{
				Logger.Error($"Failed to set window corners for {hwnd.Value}");
			}
		}
	}

	/// <inheritdoc/>
	public IEnumerable<HWND> GetAllWindows()
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

	/// <inheritdoc />
	public HDWP BeginDeferWindowPos(int nNumWindows) => PInvoke.BeginDeferWindowPos(nNumWindows);

	/// <inheritdoc />
	public HDWP DeferWindowPos(
		HDWP hWinPosInfo,
		HWND hWnd,
		HWND hWndInsertAfter,
		int x,
		int y,
		int cx,
		int cy,
		SET_WINDOW_POS_FLAGS uFlags
	) => PInvoke.DeferWindowPos(hWinPosInfo, hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);

	/// <inheritdoc />
	public BOOL EndDeferWindowPos(HDWP hWinPosInfo) => PInvoke.EndDeferWindowPos(hWinPosInfo);
}
