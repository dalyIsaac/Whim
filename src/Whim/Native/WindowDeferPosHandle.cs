using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Sets the position of multiple windows at once, using <see cref="INativeManager.DeferWindowPos"/>.
/// As stated in Raymond Chen's blog post (https://devblogs.microsoft.com/oldnewthing/20050706-26/?p=35023),
/// this reduces the amount of repainting.
/// </summary>
public sealed class WindowDeferPosHandle : IDisposable
{
	private readonly IContext _context;
	private HDWP _hWinPosInfo;
	private readonly List<IWindow> _toMinimize;
	private readonly List<IWindow> _toMaximize;
	private readonly List<IWindow> _toNormal;

	/// <summary>
	/// Create a new <see cref="WindowDeferPosHandle"/> for <paramref name="count"/> windows.
	/// This is to be used when setting the position of multiple windows at once.
	///
	/// <see cref="WindowDeferPosHandle"/> must be used in conjunction with a <c>using</c> block
	/// or statement, otherwise <see cref="INativeManager.EndDeferWindowPos"/> won't be called.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="count">The number of windows to layout.</param>
	public WindowDeferPosHandle(IContext context, int count)
	{
		_context = context;
		_hWinPosInfo = _context.NativeManager.BeginDeferWindowPos(count);

		_toMinimize = new List<IWindow>();
		_toMaximize = new List<IWindow>();
		_toNormal = new List<IWindow>();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		foreach (IWindow w in _toMinimize)
		{
			if (!w.IsMinimized)
			{
				_context.NativeManager.MinimizeWindow(w.Handle);
			}
		}

		foreach (IWindow w in _toMaximize)
		{
			if (!w.IsMaximized)
			{
				_context.NativeManager.ShowWindowMaximized(w.Handle);
			}
		}

		foreach (IWindow w in _toNormal)
		{
			_context.NativeManager.ShowWindowNoActivate(w.Handle);
		}

		_context.NativeManager.EndDeferWindowPos(_hWinPosInfo);
	}

	/// <summary>
	/// Using the given <paramref name="windowState"/>, sets the window's position.
	/// </summary>
	/// <param name="windowState"></param>
	/// <param name="hwndInsertAfter">The window handle to insert show the given window behind.</param>
	public void DeferWindowPos(IWindowState windowState, HWND? hwndInsertAfter = null)
	{
		// We use HWND_BOTTOM, as modifying the Z-order of a window
		// may cause EVENT_SYSTEM_FOREGROUND to be set, which in turn
		// causes the relevant window to be focused, when the user hasn't
		// actually changed the focus.
		hwndInsertAfter ??= (HWND)1; // HWND_BOTTOM

		IWindow window = windowState.Window;

		ILocation<int>? offset = _context.NativeManager.GetWindowOffset(window.Handle);
		if (offset is null)
		{
			return;
		}

		ILocation<int> location = windowState.Location.Add(offset);

		WindowSize windowSize = windowState.WindowSize;

		SET_WINDOW_POS_FLAGS flags =
			SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
			| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
			| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
			| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
			| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

		if (windowSize == WindowSize.Maximized)
		{
			_toMaximize.Add(window);
			flags = flags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}
		else if (windowSize == WindowSize.Minimized)
		{
			_toMinimize.Add(window);
			flags = flags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}
		else
		{
			_toNormal.Add(window);
		}

		_hWinPosInfo = _context.NativeManager.DeferWindowPos(
			_hWinPosInfo,
			window.Handle,
			(HWND)hwndInsertAfter,
			location.X,
			location.Y,
			location.Width,
			location.Height,
			flags
		);
	}

	/// <summary>
	/// Set the position of a single window.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="windowState"></param>
	/// <param name="hwndInsertAfter">The window handle to insert show the given window behind.</param>
	private static void SetWindowPos(IContext context, IWindowState windowState, HWND? hwndInsertAfter = null)
	{
		using WindowDeferPosHandle handle = new(context, 1);
		handle.DeferWindowPos(windowState, hwndInsertAfter);
	}

	/// <summary>
	/// Set the position of a single window, while accounting for any scaling issues possible.
	/// Hopefully one day this will be replaced by <see cref="SetWindowPos(IContext, IWindowState, HWND?)"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="windowState"></param>
	/// <param name="monitor"></param>
	/// <param name="hwndInsertAfter">The window handle to insert show the given window behind.</param>
	public static void SetWindowPosFixScaling(
		IContext context,
		IWindowState windowState,
		IMonitor monitor,
		HWND? hwndInsertAfter = null
	)
	{
		SetWindowPos(context, windowState, hwndInsertAfter);

		if (!monitor.IsPrimary && context.MonitorManager.PrimaryMonitor.ScaleFactor != monitor.ScaleFactor)
		{
			// We need to call SetWindowPos again, as the first call will cause the window to be scaled, and the second call
			// will cause the window to be positioned correctly.

			// NOTE: I have no idea if the comment above is true - it was generated by GitHub Copilot. Regardless, this hack works.
			// Any suggestions for a better way to do this are welcome.
			SetWindowPos(context, windowState, hwndInsertAfter);
		}
	}
}
