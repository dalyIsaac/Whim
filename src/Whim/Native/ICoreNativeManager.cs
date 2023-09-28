using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Manager for interacting with native Windows APIs, or Windows App SDK APIs, for internal Whim core use only.
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
	UnhookWinEventSafeHandle SetWinEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc);

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
	UnhookWindowsHookExSafeHandle SetWindowsHookEx(
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
	LRESULT CallNextHookEx(int nCode, WPARAM wParam, LPARAM lParam);

	/// <summary>
	/// Set the <see cref="PInvoke.GetKeyState(int)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getkeystate
	/// </summary>
	/// <param name="nVirtKey"></param>
	/// <returns></returns>
	short GetKeyState(int nVirtKey);

	/// <summary>
	/// Set the <see cref="PInvoke.GetCursorPos(out System.Drawing.Point)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos
	/// </summary>
	/// <param name="lpPoint"></param>
	/// <returns></returns>
	BOOL GetCursorPos(out IPoint<int> lpPoint);

	/// <summary>
	/// Get the coordinates for the left-side of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	int GetVirtualScreenLeft();

	/// <summary>
	/// Get the coordinates for the top-side of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	int GetVirtualScreenTop();

	/// <summary>
	/// Get the width of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	int GetVirtualScreenWidth();

	/// <summary>
	/// Get the height of the virtual screen.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	int GetVirtualScreenHeight();

	/// <summary>
	/// Whether the desktop has multiple monitors.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX)"/> with <see cref="SYSTEM_METRICS_INDEX.SM_CMONITORS"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
	/// </remarks>
	/// <returns></returns>
	bool HasMultipleMonitors();

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
	BOOL EnumDisplayMonitors(SafeHandle? hdc, RECT? lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData);

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
	BOOL GetPrimaryDisplayWorkArea(out RECT workArea);

	/// <summary>
	/// Retrieve information about a display monitor.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetMonitorInfo(HMONITOR, ref MONITORINFO)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getmonitorinfoa
	/// </remarks>
	/// <param name="hMonitor"></param>
	/// <returns></returns>
	MONITORINFO? GetMonitorInfo(HMONITOR hMonitor);

	/// <summary>
	/// Retrieve information about a display monitor.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetMonitorInfo(HMONITOR, ref MONITORINFO)"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getmonitorinfoa
	/// </remarks>
	/// <param name="hMonitor"></param>
	/// <returns></returns>
	MONITORINFOEXW? GetMonitorInfoEx(HMONITOR hMonitor);

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
	HMONITOR MonitorFromPoint(Point pt, MONITOR_FROM_FLAGS dwFlags);

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
	BOOL GetWindowRect(HWND hWnd, out RECT lpRect);

	/// <summary>
	/// Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
	/// </summary>
	/// <remarks>
	/// This uses <see cref="PInvoke.GetForegroundWindow"/> <br/>
	///
	/// For more, see https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getforegroundwindow
	/// </remarks>
	/// <returns></returns>
	HWND GetForegroundWindow();

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
	BOOL IsWindowMinimized(HWND hWnd);

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
	BOOL IsWindowMaximized(HWND hWnd);

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
	BOOL SetForegroundWindow(HWND hWnd);

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
	uint SendInput(INPUT[] pInputs, int cbSize);

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
	BOOL BringWindowToTop(HWND hWnd);

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
	uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetWindowText"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	string GetWindowText(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a splash window.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	bool IsSplashScreen(HWND hwnd);

	/// <summary>
	/// Enumerates over all the <see cref="HWND"/> of all the top-level windows.
	/// </summary>
	/// <returns></returns>
	IEnumerable<HWND> GetAllWindows();

	/// <summary>
	/// Enumerates the child windows that belong to the specified <paramref name="hWndParent"/> by passing the handle to each child window, in turn, to an application-defined callback function.
	/// </summary>
	/// <param name="hWndParent"></param>
	/// <returns></returns>
	IEnumerable<HWND> GetChildWindows(HWND hWndParent);

	/// <summary>
	/// Returns <see langword="true"/> when the window is a cloaked window.
	/// For example, and empty <c>ApplicationFrameWindow</c>.
	/// For more, see https://social.msdn.microsoft.com/Forums/vstudio/en-US/f8341376-6015-4796-8273-31e0be91da62/difference-between-actually-visible-and-not-visiblewhich-are-there-but-we-cant-see-windows-of?forum=vcgeneral
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	bool IsCloakedWindow(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a standard window.
	/// Based on https://github.com/microsoft/PowerToys/blob/fa81968dbb58a0697c45335a8f453e5794852348/src/modules/fancyzones/FancyZonesLib/FancyZones.cpp#L381
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	bool IsStandardWindow(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> when the window has no visible owner.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	bool HasNoVisibleOwner(HWND hwnd);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a system window.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <param name="className">The window's class name.</param>
	/// <returns></returns>
	bool IsSystemWindow(HWND hwnd, string className);

	/// <inheritdoc cref="PInvoke.IsWindow(HWND)"/>
	bool IsWindow(HWND hWnd);

	/// <summary>Installs or updates a window subclass callback.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> The handle of the window being subclassed.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="pfnSubclass">
	/// <para>Type: <b><a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a></b> A pointer to a window procedure. This pointer and the subclass ID uniquely identify this subclass callback. For the callback function prototype, see <a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uIdSubclass">
	/// <para>Type: <b>UINT_PTR</b> The subclass ID. This ID together with the subclass procedure uniquely identify a subclass. To remove a subclass, pass the subclass procedure and this value to the <a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nf-commctrl-removewindowsubclass">RemoveWindowSubclass</a> function. This value is passed to the subclass procedure in the uIdSubclass parameter.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="dwRefData">
	/// <para>Type: <b>DWORD_PTR</b> <b>DWORD_PTR</b> to reference data. The meaning of this value is determined by the calling application. This value is passed to the subclass procedure in the dwRefData parameter. A different dwRefData is associated with each combination of window handle, subclass procedure and uIdSubclass.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> <b>TRUE</b> if the subclass callback was successfully installed; otherwise, <b>FALSE</b>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	/// <returns></returns>
	BOOL SetWindowSubclass(HWND hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass, nuint dwRefData);

	/// <summary>Removes a subclass callback from a window.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> The handle of the window being subclassed.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-removewindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="pfnSubclass">
	/// <para>Type: <b><a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a></b> A pointer to a window procedure. This pointer and the subclass ID uniquely identify this subclass callback. For the callback function prototype, see <a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-removewindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uIdSubclass">
	/// <para>Type: <b>UINT_PTR</b> The <b>UINT_PTR</b> subclass ID. This ID and the callback pointer uniquely identify this subclass callback. Note: On 64-bit versions of Windows this is a 64-bit value.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-removewindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> <b>TRUE</b> if the subclass callback was successfully removed; otherwise, <b>FALSE</b>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/commctrl/nf-commctrl-removewindowsubclass">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	/// <returns></returns>
	BOOL RemoveWindowSubclass(HWND hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass);

	/// <summary>Calls the next handler in a window's subclass chain. The last handler in the subclass chain calls the original window procedure for the window.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> A handle to the window being subclassed.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uMsg">
	/// <para>Type: <b>UINT</b> A value of type unsigned <b>int</b> that specifies a window message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="wParam">
	/// <para>Type: <b>WPARAM</b> Specifies additional message information. The contents of this parameter depend on the value of the window message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="lParam">
	/// <para>Type: <b>LPARAM</b> Specifies additional message information. The contents of this parameter depend on the value of the window message. Note: On 64-bit versions of Windows LPARAM is a 64-bit value.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>LRESULT</b> The returned value is specific to the message sent. This value should be ignored.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	LRESULT DefSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam);

	/// <summary>Queries the dots per inch (dpi) of a display.</summary>
	/// <param name="hmonitor">Handle of the monitor being queried.</param>
	/// <param name="dpiType">The type of DPI being queried. Possible values are from the <a href="https://docs.microsoft.com/windows/desktop/api/shellscalingapi/ne-shellscalingapi-monitor_dpi_type">MONITOR_DPI_TYPE</a> enumeration.</param>
	/// <param name="dpiX">The value of the DPI along the X axis. This value always refers to the horizontal edge, even when the screen is rotated.</param>
	/// <param name="dpiY">The value of the DPI along the Y axis. This value always refers to the vertical edge, even when the screen is rotated.</param>
	/// <returns>
	/// <para>This function returns one of the following values. </para>
	/// <para>This doc was truncated.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//shellscalingapi/nf-shellscalingapi-getdpiformonitor">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	HRESULT GetDpiForMonitor(HMONITOR hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

	/// <summary>
	/// Marshals data from an unmanaged block of memory to a newly allocated managed object of the type specified by a generic type parameter.
	/// </summary>
	/// <typeparam name="T">The type of the object to which the data is to be copied. This must be a formatted class or a structure.</typeparam>
	/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
	/// <returns>A managed object that contains the data that the ptr parameter points to.</returns>
	/// <exception cref="ArgumentException">The layout of T is not sequential or explicit.</exception>
	/// <exception cref="MissingMethodException">The class specified by T does not have an accessible parameterless constructor.</exception>
	T? PtrToStructure<T>(nint ptr)
		where T : struct;

	/// <summary>Registers the specified window to receive session change notifications.</summary>
	/// <param name="hWnd">Handle of the window to receive session change notifications.</param>
	/// <param name="dwFlags">
	/// <para>Specifies which session notifications are to be received. This parameter can be one of the following values.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//wtsapi32/nf-wtsapi32-wtsregistersessionnotification#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>If the function succeeds, the return value is <b>TRUE</b>. Otherwise, it is <b>FALSE</b>. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//wtsapi32/nf-wtsapi32-wtsregistersessionnotification">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	BOOL WTSRegisterSessionNotification(HWND hWnd, uint dwFlags);

	/// <summary>Unregisters the specified window so that it receives no further session change notifications.</summary>
	/// <param name="hWnd">Handle of the window to be unregistered from receiving session notifications.</param>
	/// <returns>
	/// <para>If the function succeeds, the return value is <b>TRUE</b>. Otherwise, it is <b>FALSE</b>. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//wtsapi32/nf-wtsapi32-wtsunregistersessionnotification">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	BOOL WTSUnRegisterSessionNotification(HWND hWnd);

	/// <summary>
	/// Adds a task to the <see cref="DispatcherQueue" /> which will be executed on the thread associated
	/// with the <see cref="DispatcherQueue" />.
	/// </summary>
	/// <param name="callback">The task to execute.</param>
	/// <returns><see langword="true" /> indicates that the task was added to the queue; <see langword="false" />, otherwise.</returns>
	bool TryEnqueue(DispatcherQueueHandler callback);

	/// <summary>
	/// Queues the specified work to run on the thread pool and returns a <see cref="Task{TResult}"/> object that
	/// represents that work.
	///
	/// The cleanup callback is called on the UI thread when the task completes.
	/// </summary>
	/// <typeparam name="TWorkResult">Specifies the type of the result returned by the work callback.</typeparam>
	/// <param name="work">The work to execute asynchronously.</param>
	/// <param name="cleanup">The cleanup callback to execute when the task completes.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
	/// <returns>A <see cref="Task{TResult}"/> object that represents the work queued to execute in the thread pool.</returns>
	Task RunTask<TWorkResult>(
		Func<TWorkResult> work,
		Action<Task<TWorkResult>> cleanup,
		CancellationToken cancellationToken
	);

	/// <summary>
	/// Gets a <see cref="HWND" /> for the current window to use for the <see cref="WindowMessageMonitor" />.
	/// </summary>
	/// <remarks>
	/// The first time this is called, the window will be created.
	/// </remarks>
	/// <returns>The <see cref="HWND" /> of the window.</returns>
	HWND WindowMessageMonitorWindowHandle { get; }

	/// <summary>
	/// Get the process name and process path for the given process ID.
	/// </summary>
	/// <param name="processId">The process ID.</param>
	/// <returns>The process name and process path.</returns>
	(string processName, string? processPath) GetProcessNameAndPath(int processId);

	/// <summary>Retrieves the specified value from the WNDCLASSEX structure associated with the specified window.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> A handle to the window and, indirectly, the class to which the window belongs.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getclasslongptrw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="nIndex">Type: <b>int</b></param>
	/// <returns>
	/// <para>Type: <b>ULONG_PTR</b> If the function succeeds, the return value is the requested value. If the function fails, the return value is zero. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getclasslongptrw">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	nuint GetClassLongPtr(HWND hWnd, GET_CLASS_LONG_INDEX nIndex);

	/// <summary>Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> A handle to the window whose window procedure will receive the message. If this parameter is <b>HWND_BROADCAST</b> ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows. Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="Msg">
	/// <para>Type: <b>UINT</b> The message to be sent. For lists of the system-provided messages, see <a href="https://docs.microsoft.com/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</a>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="wParam">
	/// <para>Type: <b>WPARAM</b> Additional message-specific information.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="lParam">
	/// <para>Type: <b>LPARAM</b> Additional message-specific information.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>LRESULT</b> The return value specifies the result of the message processing; it depends on the message sent.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	LRESULT SendMessage(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);

	/// <inheritdoc cref="PInvoke.GetClientRect(HWND, RECT*)"/>
	BOOL GetClientRect(HWND hWnd, out RECT lpRect);

	/// <inheritdoc cref="PInvoke.CreateSolidBrush(COLORREF)"/>
	DeleteObjectSafeHandle CreateSolidBrush(COLORREF color);

	/// <summary>
	/// Enables the blur effect for the given <paramref name="hwnd"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns>Whether the function succeeded.</returns>
	bool EnableBlurBehindWindow(HWND hwnd);

	/// <inheritdoc cref="PInvoke.FillRect(HDC, in RECT, SafeHandle)"/>
	int FillRect(HDC hDC, in RECT lprc, SafeHandle hbr);

	/// <summary>
	/// Gets the <see cref="Icon"/> for the given <paramref name="hIcon"/>.
	/// </summary>
	/// <param name="hIcon"></param>
	Icon LoadIconFromHandle(nint hIcon);

	/// <inheritdoc cref="PInvoke.MonitorFromWindow(HWND, MONITOR_FROM_FLAGS)"/>
	HMONITOR MonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags);
}
