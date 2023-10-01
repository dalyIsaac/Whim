using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class CoreNativeManager : ICoreNativeManager
{
	private readonly IContext _context;

	public CoreNativeManager(IContext context)
	{
		_context = context;
	}

	public UnhookWinEventSafeHandle SetWinEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc) =>
		PInvoke.SetWinEventHook(eventMin, eventMax, null, lpfnWinEventProc, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);

	public UnhookWindowsHookExSafeHandle SetWindowsHookEx(
		WINDOWS_HOOK_ID idHook,
		HOOKPROC lpfn,
		SafeHandle? hmod,
		uint dwThreadId
	) => PInvoke.SetWindowsHookEx(idHook, lpfn, hmod, dwThreadId);

	public LRESULT CallNextHookEx(int nCode, WPARAM wParam, LPARAM lParam) =>
		PInvoke.CallNextHookEx(null, nCode, wParam, lParam);

	public short GetKeyState(int nVirtKey) => PInvoke.GetKeyState(nVirtKey);

	public BOOL GetCursorPos(out IPoint<int> lpPoint)
	{
		bool res = PInvoke.GetCursorPos(out Point point);
		lpPoint = new Point<int>(point.X, point.Y);
		return res;
	}

	public int GetVirtualScreenLeft() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);

	public int GetVirtualScreenTop() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);

	public int GetVirtualScreenWidth() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);

	public int GetVirtualScreenHeight() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);

	public bool HasMultipleMonitors() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CMONITORS) != 0;

	public BOOL EnumDisplayMonitors(SafeHandle? hdc, RECT? lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData) =>
		PInvoke.EnumDisplayMonitors(new HDC(hdc?.DangerousGetHandle() ?? 0), lprcClip, lpfnEnum, dwData);

	public BOOL GetPrimaryDisplayWorkArea(out RECT lpRect)
	{
		RECT rect = default;
		BOOL result;
		unsafe
		{
			result = PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA, 0, &rect, 0);
		}

		lpRect = rect;
		return result;
	}

	public MONITORINFOEXW? GetMonitorInfoEx(HMONITOR hMonitor)
	{
		unsafe
		{
			MONITORINFOEXW infoEx = new() { monitorInfo = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFOEXW) } };
			MONITORINFOEXW* infoExPtr = &infoEx;
			return PInvoke.GetMonitorInfo(hMonitor, (MONITORINFO*)infoExPtr) ? infoEx : null;
		}
	}

	public HMONITOR MonitorFromPoint(Point pt, MONITOR_FROM_FLAGS dwFlags) => PInvoke.MonitorFromPoint(pt, dwFlags);

	public BOOL GetWindowRect(HWND hWnd, out RECT lpRect) => PInvoke.GetWindowRect(hWnd, out lpRect);

	public HWND GetForegroundWindow() => PInvoke.GetForegroundWindow();

	public BOOL IsWindowMinimized(HWND hWnd) => PInvoke.IsIconic(hWnd);

	public BOOL IsWindowMaximized(HWND hWnd) => PInvoke.IsZoomed(hWnd);

	public BOOL SetForegroundWindow(HWND hWnd) => PInvoke.SetForegroundWindow(hWnd);

	/// <inheritdoc />
	public uint SendInput(INPUT[] pInputs, int cbSize) => PInvoke.SendInput(pInputs, cbSize);

	public BOOL BringWindowToTop(HWND hWnd) => PInvoke.BringWindowToTop(hWnd);

	public uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId)
	{
		uint pid;
		uint creator;
		unsafe
		{
			creator = PInvoke.GetWindowThreadProcessId(hWnd, &pid);
		}
		lpdwProcessId = pid;
		return creator;
	}

	private const int _bufferCapacity = 255;

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

	public bool IsSplashScreen(HWND hwnd)
	{
		string classname = _context.NativeManager.GetClassName(hwnd);
		if (classname.Length == 0)
		{
			return false;
		}

		return classname == _splashClassName;
	}

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

	public IEnumerable<HWND> GetChildWindows(HWND hwnd)
	{
		List<HWND> windows = new();

		PInvoke.EnumChildWindows(
			hwnd,
			(handle, param) =>
			{
				windows.Add(handle);
				return (BOOL)true;
			},
			0
		);

		return windows;
	}

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

		string className = _context.NativeManager.GetClassName(hwnd);
		if (IsSystemWindow(hwnd, className))
		{
			return false;
		}

		return true;
	}

	private readonly HashSet<string> _systemClasses =
		new() { "SysListView32", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Progman" };

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

	public bool IsWindow(HWND hWnd) => PInvoke.IsWindow(hWnd);

	public bool HasNoVisibleOwner(HWND hwnd)
	{
		HWND owner = PInvoke.GetWindow(hwnd, GET_WINDOW_CMD.GW_OWNER);

		// The following warning was disabled, since GetWindow can return null, per
		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindow
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
		if (owner == null)
		{
			return true; // There is no owner at all
		}
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

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

	public bool IsCloakedWindow(HWND hwnd)
	{
		unsafe
		{
			int cloaked;
			HRESULT res = PInvoke.DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, &cloaked, sizeof(int));
			return cloaked != 0 || res.Failed;
		}
	}

	public BOOL SetWindowSubclass(HWND hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass, nuint dwRefData) =>
		PInvoke.SetWindowSubclass(hWnd, pfnSubclass, uIdSubclass, dwRefData);

	public BOOL RemoveWindowSubclass(HWND hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass) =>
		PInvoke.RemoveWindowSubclass(hWnd, pfnSubclass, uIdSubclass);

	public LRESULT DefSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam) =>
		PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);

	public HRESULT GetDpiForMonitor(HMONITOR hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY) =>
		PInvoke.GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY);

	public T? PtrToStructure<T>(IntPtr ptr)
		where T : struct => Marshal.PtrToStructure<T>(ptr);

	public BOOL WTSRegisterSessionNotification(HWND hWnd, uint dwFlags) =>
		PInvoke.WTSRegisterSessionNotification(hWnd, dwFlags);

	public BOOL WTSUnRegisterSessionNotification(HWND hWnd) => PInvoke.WTSUnRegisterSessionNotification(hWnd);

	public bool TryEnqueue(DispatcherQueueHandler callback) =>
		DispatcherQueue.GetForCurrentThread().TryEnqueue(callback);

	public Task RunTask<TWorkResult>(
		Func<TWorkResult> work,
		Action<Task<TWorkResult>> cleanup,
		CancellationToken cancellationToken
	)
	{
		TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		return Task.Run(work, cancellationToken)
			.ContinueWith(cleanup, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, uiScheduler);
	}

	private Microsoft.UI.Xaml.Window? _window;

	public HWND WindowMessageMonitorWindowHandle
	{
		get
		{
			if (_window == null)
			{
				_window = new();
				_window.SetIsShownInSwitchers(false);
			}

			return _window.GetHandle();
		}
	}

	public (string processName, string? processPath) GetProcessNameAndPath(int processId)
	{
		using Process process = Process.GetProcessById(processId);
		return (process.ProcessName, process.MainModule?.FileName);
	}

	public nuint GetClassLongPtr(HWND hWnd, GET_CLASS_LONG_INDEX nIndex) => PInvoke.GetClassLongPtr(hWnd, nIndex);

	public LRESULT SendMessage(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) =>
		PInvoke.SendMessage(hWnd, Msg, wParam, lParam);

	public BOOL GetClientRect(HWND hWnd, out RECT lpRect) => PInvoke.GetClientRect(hWnd, out lpRect);

	public DeleteObjectSafeHandle CreateSolidBrush(COLORREF crColor) => PInvoke.CreateSolidBrush_SafeHandle(crColor);

	public bool EnableBlurBehindWindow(HWND hwnd)
	{
		using DeleteObjectSafeHandle rgn = PInvoke.CreateRectRgn_SafeHandle(-2, -2, -1, -1);
		return PInvoke
			.DwmEnableBlurBehindWindow(
				hwnd,
				new DWM_BLURBEHIND()
				{
					dwFlags = PInvoke.DWM_BB_ENABLE | PInvoke.DWM_BB_BLURREGION,
					fEnable = true,
					hRgnBlur = new HRGN(rgn.DangerousGetHandle())
				}
			)
			.Succeeded;
	}

	public int FillRect(HDC hDC, in RECT lprc, SafeHandle hbr) => PInvoke.FillRect(hDC, lprc, hbr);

	public Icon LoadIconFromHandle(nint hIcon) => Icon.FromHandle(hIcon);

	public HMONITOR MonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags) =>
		PInvoke.MonitorFromWindow(hwnd, dwFlags);
}
