using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Manager for interacting with native Windows APIs, for internal Whim core use only.
/// </summary>
internal interface ICoreNativeManager
{
	/// <summary>
	/// Set the <see cref="PInvoke.SetWinEventHook(uint, uint, SafeHandle, WINEVENTPROC, uint, uint, uint)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-setwineventhook
	/// </summary>
	/// <param name="eventMin"></param>
	/// <param name="eventMax"></param>
	/// <param name="lpfnWinEventProc"></param>
	/// <returns></returns>
	public UnhookWinEventSafeHandle SetWinEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc);

	/// <summary>
	/// Set the <see cref="PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID, HOOKPROC, SafeHandle, uint)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowshookexa
	/// </summary>
	/// <param name="idHook"></param>
	/// <param name="lpfn"></param>
	/// <param name="hmod"></param>
	/// <param name="dwThreadId"></param>
	/// <returns></returns>
	public UnhookWindowsHookExSafeHandle SetWindowsHookEx(
		WINDOWS_HOOK_ID idHook,
		HOOKPROC lpfn,
		SafeHandle? hmod,
		uint dwThreadId
	);

	/// <summary>
	/// Set the <see cref="PInvoke.CallNextHookEx(SafeHandle, int, WPARAM, LPARAM)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-callnexthookex
	/// </summary>
	/// <param name="nCode"></param>
	/// <param name="wParam"></param>
	/// <param name="lParam"></param>
	/// <returns></returns>
	public LRESULT CallNextHookEx(int nCode, WPARAM wParam, LPARAM lParam);

	/// <summary>
	/// Set the <see cref="PInvoke.GetKeyState(int)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getkeystate
	/// </summary>
	/// <param name="nVirtKey"></param>
	/// <returns></returns>
	public short GetKeyState(int nVirtKey);

	/// <summary>
	/// Set the <see cref="PInvoke.GetCursorPos(out System.Drawing.Point)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos
	/// </summary>
	/// <param name="lpPoint"></param>
	/// <returns></returns>
	public BOOL GetCursorPos(out System.Drawing.Point lpPoint);

	/// <summary>
	/// Get the coordinates for the left-side of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	public int GetVirtualScreenLeft();

	/// <summary>
	/// Get the coordinates for the top-side of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	public int GetVirtualScreenTop();

	/// <summary>
	/// Get the width of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	public int GetVirtualScreenWidth();

	/// <summary>
	/// Get the height of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	public int GetVirtualScreenHeight();

	/// <summary>
	/// Whether the desktop has multiple monitors.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_CMONITORS"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	public bool HasMultipleMonitors();

	/// <summary>
	/// Enumerates display monitors (including invisible pseudo-monitors associated with the mirroring drivers) that intersect a region formed by the intersection of a specified clipping rectangle and the visible region of a device context.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.EnumDisplayMonitors(HDC, RECT*, MONITORENUMPROC, LPARAM)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors
	/// </remarks>
	/// <param name="hdc"></param>
	/// <param name="lprcClip"></param>
	/// <param name="lpfnEnum"></param>
	/// <param name="dwData"></param>
	/// <returns></returns>
	public BOOL EnumDisplayMonitors(SafeHandle? hdc, RECT? lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData);

	/// <summary>
	/// Gets the scale factor of a specific monitor.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetScaleFactorForMonitor(HMONITOR, out Windows.Win32.UI.Shell.Common.DEVICE_SCALE_FACTOR)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/shellscalingapi/nf-shellscalingapi-getscalefactorformonitor
	/// </remarks>
	/// <param name="hMonitor"></param>
	/// <param name="scaleFactor"></param>
	/// <returns></returns>
	public HRESULT GetScaleFactorForMonitor(
		HMONITOR hMonitor,
		out Windows.Win32.UI.Shell.Common.DEVICE_SCALE_FACTOR scaleFactor
	);

	/// <summary>
	/// Retrieves the size of the work area on the primary display monitor.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION, uint, void*, SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS)"/> with <see cref="SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-systemparametersinfoa
	/// </remarks>
	/// <param name="workArea"></param>
	/// <returns></returns>
	public BOOL GetPrimaryDisplayWorkArea(out RECT workArea);

	/// <summary>
	/// Retrieve information about a display monitor.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetMonitorInfo(HMONITOR, ref MONITORINFO)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getmonitorinfoa
	/// </remarks>
	/// <param name="hMonitor"></param>
	/// <param name="lpmi"></param>
	/// <returns></returns>
	public BOOL GetMonitorInfo(HMONITOR hMonitor, ref MONITORINFO lpmi);

	/// <summary>
	/// Retrieve information about a display monitor.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetMonitorInfo(HMONITOR, ref MONITORINFO)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getmonitorinfoa
	/// </remarks>
	/// <param name="hMonitor"></param>
	/// <param name="lpmi"></param>
	/// <returns></returns>
	public unsafe BOOL GetMonitorInfo(HMONITOR hMonitor, ref MONITORINFOEXW lpmi);

	/// <summary>
	/// Retrieve a handle to the display monitor that contains a specified point.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.MonitorFromPoint(Point, MONITOR_FROM_FLAGS)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-monitorfrompoint
	/// </remarks>
	/// <param name="pt"></param>
	/// <param name="dwFlags"></param>
	/// <returns></returns>
	public HMONITOR MonitorFromPoint(Point pt, MONITOR_FROM_FLAGS dwFlags);

	/// <summary>
	/// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetWindowRect(HWND, out RECT)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowrect
	/// </remarks>
	/// <param name="hWnd"></param>
	/// <param name="lpRect"></param>
	/// <returns></returns>
	public BOOL GetWindowRect(HWND hWnd, out RECT lpRect);

	/// <summary>
	/// Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetForegroundWindow"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getforegroundwindow
	/// </remarks>
	/// <returns></returns>
	public HWND GetForegroundWindow();

	/// <summary>
	/// Determines whether the specified window is minimized (iconic).
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.IsIconic(HWND)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-isiconic
	/// </remarks>
	/// <param name="hWnd"></param>
	/// <returns></returns>
	public BOOL IsWindowMinimized(HWND hWnd);

	/// <summary>
	/// Determines whether a window is maximized.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.IsZoomed(HWND)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-iszoomed
	/// </remarks>
	/// <param name="hWnd"></param>
	/// <returns></returns>
	public BOOL IsWindowMaximized(HWND hWnd);

	/// <summary>
	/// Brings the thread that created the specified window into the foreground and activates the window. Keyboard input is directed to the window, and various visual cues are changed for the user. The system assigns a slightly higher priority to the thread that created the foreground window than it does to other threads.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.SetForegroundWindow(HWND)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-setforegroundwindow
	/// </remarks>
	/// <param name="hWnd"></param>
	/// <returns></returns>
	public BOOL SetForegroundWindow(HWND hWnd);

	/// <summary>
	/// Synthesizes keystrokes, mouse motions, and button clicks.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.SendInput(Span{INPUT}, int)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput
	/// </remarks>
	/// <param name="pInputs"></param>
	/// <param name="cbSize"></param>
	/// <returns></returns>
	public uint SendInput(Span<INPUT> pInputs, int cbSize);

	/// <summary>
	/// Brings the specified window to the top of the Z order. If the window is a top-level window, it is activated. If the window is a child window, the top-level parent window associated with the child window is activated.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.BringWindowToTop(HWND)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowpos
	/// </remarks>
	/// <param name="hWnd"></param>
	/// <returns></returns>
	public BOOL BringWindowToTop(HWND hWnd);

	/// <summary>
	/// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetWindowThreadProcessId(HWND, uint*)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid
	/// </remarks>
	/// <param name="hWnd"></param>
	/// <param name="lpdwProcessId"></param>
	/// <returns></returns>
	public uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetWindowText"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public string GetWindowText(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a splash window.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public bool IsSplashScreen(HWND hwnd);

	/// <summary>
	/// Enumerates over all the <see cref="HWND"/> of all the top-level windows.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<HWND> GetAllWindows();

	/// <summary>
	/// Returns <see langword="true"/> when the window is a cloaked window.
	/// For example, and empty <c>ApplicationFrameWindow</c>.
	/// For more, see https://social.msdn.microsoft.com/Forums/vstudio/en-US/f8341376-6015-4796-8273-31e0be91da62/difference-between-actually-visible-and-not-visiblewhich-are-there-but-we-cant-see-windows-of?forum=vcgeneral
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public bool IsCloakedWindow(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a standard window.
	/// Based on https://github.com/microsoft/PowerToys/blob/fa81968dbb58a0697c45335a8f453e5794852348/src/modules/fancyzones/FancyZonesLib/FancyZones.cpp#L381
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public bool IsStandardWindow(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> when the window has no visible owner.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	public bool HasNoVisibleOwner(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a system window.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <param name="className">The window's class name.</param>
	/// <returns></returns>
	public bool IsSystemWindow(HWND hwnd, string className);
}
