using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Accessibility;

namespace Whim;

/// <summary>
/// Manager for interacting with native Windows APIs.
/// </summary>
public interface INativeManager
{
	/// <summary>
	/// Quit the window.
	/// </summary>
	/// <param name="window"></param>
	public static abstract void QuitWindow(IWindow window);

	/// <summary>
	/// Force the window to the foreground.
	/// </summary>
	/// <param name="window"></param>
	public static abstract void ForceForegroundWindow(IWindow window);

	/// <summary>
	/// Hides the window of the associated handle.
	/// </summary>
	/// <param name="window"></param>
	public static abstract bool HideWindow(IWindow window);

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	public static abstract bool ShowWindowMaximized(IWindow window);

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	public static abstract bool ShowWindowMinimized(IWindow window);

	/// <summary>
	/// Minimizes the specified window and activates the next top-level window in the Z order.
	/// </summary>
	public static abstract bool MinimizeWindow(IWindow window);

	/// <summary>
	/// Displays a window in its most recent size and position. The window is not activated.
	/// </summary>
	public static abstract bool ShowWindowNoActivate(IWindow window);

	/// <summary>
	/// Set the <see cref="PInvoke.SetWinEventHook(uint, uint, SafeHandle, WINEVENTPROC, uint, uint, uint)"/> <br/>.
	///
	/// For more, see https://docs.microsoft.com/en-au/windows/win32/api/winuser/nf-winuser-setwineventhook
	/// </summary>
	/// <param name="eventMin"></param>
	/// <param name="eventMax"></param>
	/// <param name="lpfnWinEventProc"></param>
	/// <returns></returns>
	public static abstract UnhookWinEventSafeHandle SetWindowsEventHook(
		uint eventMin,
		uint eventMax,
		WINEVENTPROC lpfnWinEventProc
	);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetClassName"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract string GetClassName(IWindow window);

	/// <summary>
	/// Safe wrapper around <see cref="PInvoke.GetWindowText"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract string GetWindowText(IWindow window);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a splash window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract bool IsSplashScreen(IWindow window);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a standard window.
	/// Based on https://github.com/microsoft/PowerToys/blob/fa81968dbb58a0697c45335a8f453e5794852348/src/modules/fancyzones/FancyZonesLib/FancyZones.cpp#L381
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract bool IsStandardWindow(IWindow window);

	/// <summary>
	/// Hides the caption buttons from the given window.
	/// </summary>
	/// <param name="window"></param>
	public static abstract void HideCaptionButtons(IWindow window);

	/// <summary>
	/// Returns <see langword="true"/> if the window is a system window.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="className">The window's class name.</param>
	/// <returns></returns>
	public static abstract bool IsSystemWindow(IWindow window, string className);

	/// <summary>
	/// Returns <see langword="true"/> when the window has no visible owner.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract bool HasNoVisibleOwner(IWindow window);

	/// <summary>
	/// Returns <see langword="true"/> when the window is a cloaked window.
	/// For example, and empty <c>ApplicationFrameWindow</c>.
	/// For more, see https://social.msdn.microsoft.com/Forums/vstudio/en-US/f8341376-6015-4796-8273-31e0be91da62/difference-between-actually-visible-and-not-visiblewhich-are-there-but-we-cant-see-windows-of?forum=vcgeneral
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract bool IsCloakedWindow(IWindow window);

	/// <summary>
	/// Returns the window's offset.<br/>
	/// This is based on the issue raised at https://github.com/workspacer/workspacer/issues/139,
	/// and the associated fix from https://github.com/workspacer/workspacer/pull/146.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract ILocation<int> GetWindowOffset(IWindow window);

	/// <summary>
	/// Returns the window's location from DWM.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static abstract ILocation<int>? DwmGetWindowLocation(IWindow window);

	/// <summary>
	/// Sets the preferred window corners for the given <paramref name="window"/>.
	/// By default, the window corners are rounded.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="preference"></param>
	public static abstract void SetWindowCorners(
		IWindow window,
		DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
	);

	/// <summary>
	/// Enumerates over all the <see cref="HWND"/> of all the top-level windows.
	/// </summary>
	/// <returns></returns>
	public static abstract IEnumerable<HWND> GetAllWindows();
}
