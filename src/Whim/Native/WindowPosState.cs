using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// The positioning state of a window.
/// </summary>
public record WindowPosState
{
	/// <summary>
	/// The window, its location and size.
	/// </summary>
	public IWindowState WindowState;

	/// <summary>
	/// The <see cref="HWND"/> to insert the window after. See <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos">SetWindowPos</see>.
	/// </summary>
	public HWND HwndInsertAfter;

	/// <summary>
	/// The window position flags to apply.
	/// </summary>
	public SET_WINDOW_POS_FLAGS? Flags;

	/// <summary>
	/// Creates a WindowPosState record.
	/// </summary>
	/// <param name="windowState"></param>
	/// <param name="hwndInsertAfter">
	/// The <see cref="HWND"/> to insert the window after. See <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos">SetWindowPos</see>.
	/// Defaults to <c>new HWND(1)</c>.
	/// </param>
	/// <param name="flags"></param>
	public WindowPosState(IWindowState windowState, HWND? hwndInsertAfter = null, SET_WINDOW_POS_FLAGS? flags = null)
	{
		WindowState = windowState;
		HwndInsertAfter = hwndInsertAfter ?? new(1);
		Flags = flags;
	}
}
