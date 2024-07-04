using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// The positioning state of a window.
/// </summary>
public record DeferWindowPosState : WindowPosition
{
	/// <summary>
	/// The handle of the window to set the position for.
	/// </summary>
	public HWND Handle { get; }

	/// <summary>
	/// The <see cref="HWND"/> to insert the window after. See <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos">SetWindowPos</see>.
	/// </summary>
	public HWND HandleInsertAfter { get; }

	/// <summary>
	/// The window position flags to apply.
	/// </summary>
	public SET_WINDOW_POS_FLAGS? Flags { get; }

	/// <summary>
	/// Creates a WindowPosState record.
	/// </summary>
	/// <param name="handle">
	/// The handle of the window set the position of.
	/// </param>
	/// <param name="windowSize">
	/// The size of the window to set the position of.
	/// </param>
	/// <param name="rect">
	/// The size and position of the window.
	/// </param>
	/// <param name="hwndInsertAfter">
	/// The <see cref="HWND"/> to insert the window after. See <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos">SetWindowPos</see>.
	/// Defaults to <c>new HWND(1)</c>.
	/// </param>
	/// <param name="flags">
	/// The flags to use when setting the window position. This overrides the default flags Whim sets,
	/// except when the window is maximized or minimized.
	/// </param>
	public DeferWindowPosState(
		HWND handle,
		WindowSize windowSize,
		IRectangle<int> rect,
		HWND? hwndInsertAfter = null,
		SET_WINDOW_POS_FLAGS? flags = null
	)
		: base(windowSize, rect)
	{
		Handle = handle;
		HandleInsertAfter = hwndInsertAfter ?? new(1);
		Flags = flags;
	}
}
