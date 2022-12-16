using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

internal class CoreNativeManager : ICoreNativeManager
{
	/// <inheritdoc/>
	public UnhookWinEventSafeHandle SetWinEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc) =>
		PInvoke.SetWinEventHook(eventMin, eventMax, null, lpfnWinEventProc, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);

	/// <inheritdoc/>
	public UnhookWindowsHookExSafeHandle SetWindowsHookEx(
		WINDOWS_HOOK_ID idHook,
		HOOKPROC lpfn,
		SafeHandle? hmod,
		uint dwThreadId
	) => PInvoke.SetWindowsHookEx(idHook, lpfn, hmod, dwThreadId);

	/// <inheritdoc/>
	public LRESULT CallNextHookEx(int nCode, WPARAM wParam, LPARAM lParam) =>
		PInvoke.CallNextHookEx(null, nCode, wParam, lParam);

	/// <inheritdoc/>
	public short GetKeyState(int nVirtKey) => PInvoke.GetKeyState(nVirtKey);

	/// <inheritdoc/>
	public BOOL GetCursorPos(out Point lpPoint) => PInvoke.GetCursorPos(out lpPoint);

	/// <inheritdoc/>
	public int GetVirtualScreenLeft() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);

	/// <inheritdoc/>
	public int GetVirtualScreenTop() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);

	/// <inheritdoc/>
	public int GetVirtualScreenWidth() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);

	/// <inheritdoc/>
	public int GetVirtualScreenHeight() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);

	/// <inheritdoc/>
	public bool HasMultipleMonitors() => PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CMONITORS) != 0;

	/// <inheritdoc/>
	public BOOL EnumDisplayMonitors(SafeHandle? hdc, RECT? lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData) =>
		PInvoke.EnumDisplayMonitors(hdc, lprcClip, lpfnEnum, dwData);

	/// <inheritdoc/>
	public HRESULT GetScaleFactorForMonitor(HMONITOR hmonitor, out DEVICE_SCALE_FACTOR pdpiScale) =>
		PInvoke.GetScaleFactorForMonitor(hmonitor, out pdpiScale);

	/// <inheritdoc/>
	public unsafe BOOL SystemParametersInfo(
		SYSTEM_PARAMETERS_INFO_ACTION uiAction,
		uint uiParam,
		void* pvParam,
		SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS fWinIni
	) => PInvoke.SystemParametersInfo(uiAction, uiParam, pvParam, fWinIni);

	/// <inheritdoc/>
	public BOOL GetMonitorInfo(HMONITOR hMonitor, ref MONITORINFO lpmi) => PInvoke.GetMonitorInfo(hMonitor, ref lpmi);

	/// <inheritdoc/>
	public unsafe BOOL GetMonitorInfo(HMONITOR hMonitor, ref MONITORINFOEXW lpmi)
	{
		fixed (MONITORINFOEXW* lmpiLocal = &lpmi)
		{
			return PInvoke.GetMonitorInfo(hMonitor, (MONITORINFO*)lmpiLocal);
		}
	}

	/// <inheritdoc/>
	public HMONITOR MonitorFromPoint(Point pt, MONITOR_FROM_FLAGS dwFlags) => PInvoke.MonitorFromPoint(pt, dwFlags);

	/// <inheritdoc/>
	public BOOL GetWindowRect(HWND hWnd, out RECT lpRect) => PInvoke.GetWindowRect(hWnd, out lpRect);

	/// <inheritdoc/>
	public HWND GetForegroundWindow() => PInvoke.GetForegroundWindow();

	/// <inheritdoc/>
	public BOOL IsWindowMinimized(HWND hWnd) => PInvoke.IsIconic(hWnd);

	/// <inheritdoc/>
	public BOOL IsWindowMaximized(HWND hWnd) => PInvoke.IsZoomed(hWnd);

	/// <inheritdoc/>
	public BOOL SetForegroundWindow(HWND hWnd) => PInvoke.SetForegroundWindow(hWnd);

	/// <inheritdoc />
	public uint SendInput(Span<INPUT> pInputs, int cbSize) => PInvoke.SendInput(pInputs, cbSize);

	/// <inheritdoc/>
	public BOOL BringWindowToTop(HWND hWnd) => PInvoke.BringWindowToTop(hWnd);

	/// <inheritdoc/>
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
}
