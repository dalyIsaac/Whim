namespace Windows.Win32
{
	namespace UI.WindowsAndMessaging
	{
		/// <summary>
		/// Flags for <see cref="PInvoke.SetWindowPos(Foundation.HWND, Foundation.HWND, int, int, int, int, SET_WINDOW_POS_FLAGS)"/>.
		///
		/// See <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos"/>.
		/// </summary>
		[Flags]
		public enum SET_WINDOW_POS_FLAGS : uint
		{
			/// <summary>
			/// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
			/// </summary>
			SWP_ASYNCWINDOWPOS = 0x00004000,

			/// <summary>
			/// Prevents generation of the WM_SYNCPAINT message.
			/// </summary>
			SWP_DEFERERASE = 0x00002000,

			/// <summary>
			/// Draws a frame (defined in the window's class description) around the window.
			/// </summary>
			SWP_DRAWFRAME = 0x00000020,

			/// <summary>
			/// Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
			/// </summary>
			SWP_FRAMECHANGED = 0x00000020,

			/// <summary>
			/// Hides the window.
			/// </summary>
			SWP_HIDEWINDOW = 0x00000080,

			/// <summary>
			/// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
			/// </summary>
			SWP_NOACTIVATE = 0x00000010,

			/// <summary>
			/// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
			/// </summary>
			SWP_NOCOPYBITS = 0x00000100,

			/// <summary>
			/// Retains the current position (ignores X and Y parameters).
			/// </summary>
			SWP_NOMOVE = 0x00000002,

			/// <summary>
			/// Does not change the owner window's position in the Z order.
			/// </summary>
			SWP_NOOWNERZORDER = 0x00000200,

			/// <summary>
			/// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
			/// </summary>
			SWP_NOREDRAW = 0x00000008,

			/// <summary>
			/// Same as the SWP_NOOWNERZORDER flag.
			/// </summary>
			SWP_NOREPOSITION = 0x00000200,

			/// <summary>
			/// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
			/// </summary>
			SWP_NOSENDCHANGING = 0x00000400,

			/// <summary>
			/// Retains the current size (ignores the cx and cy parameters).
			/// </summary>
			SWP_NOSIZE = 0x00000001,

			/// <summary>
			/// Retains the current Z order (ignores the hWndInsertAfter parameter).
			/// </summary>
			SWP_NOZORDER = 0x00000004,

			/// <summary>
			/// Displays the window.
			/// </summary>
			SWP_SHOWWINDOW = 0x00000040,
		}
	}
}
