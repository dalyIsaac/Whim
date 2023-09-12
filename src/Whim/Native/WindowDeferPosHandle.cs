using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Sets the position of multiple windows at once, using <see cref="INativeManager.DeferWindowPos"/>.
/// As stated in Raymond Chen's blog post (https://devblogs.microsoft.com/oldnewthing/20050706-26/?p=35023),
/// this reduces the amount of repainting.
///
/// However, when the system has non-100% scaled monitors, for some reason we need to set the window
/// position twice, otherwise windows will have incorrect dimensions.
/// </summary>
public sealed class WindowDeferPosHandle : IDisposable
{
	private readonly IContext _context;
	private readonly CancellationToken? _cancellationToken;
	private readonly List<(IWindowState windowState, HWND hwndInsertAfter, SET_WINDOW_POS_FLAGS? flags)> _windowStates =
		new();

	/// <summary>
	/// The default flags to use when setting the window position.
	/// </summary>
	private const SET_WINDOW_POS_FLAGS DefaultFlags =
		SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
		| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
		| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
		| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

	/// <summary>
	/// Create a new <see cref="WindowDeferPosHandle"/> to set the position of multiple windows at once.
	///
	/// <see cref="WindowDeferPosHandle"/> must be used in conjunction with a <c>using</c> block,
	/// a <c>using</c> statement, or by calling <see cref="Dispose"/> manually, otherwise
	/// <see cref="INativeManager.EndDeferWindowPos"/> won't be called.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> that can be used to lay out the windows.
	/// </param>
	public WindowDeferPosHandle(IContext context, CancellationToken? cancellationToken = null)
	{
		Logger.Debug("Creating new WindowDeferPosHandle");
		_context = context;
		_cancellationToken = cancellationToken;
	}

	/// <summary>
	/// Create a new <see cref="WindowDeferPosHandle"/> to set the position of multiple windows at once.
	///
	/// <see cref="WindowDeferPosHandle"/> must be used in conjunction with a <c>using</c> block,
	/// a <c>using</c> statement, or by calling <see cref="Dispose"/> manually, otherwise
	/// <see cref="INativeManager.EndDeferWindowPos"/> won't be called.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="windowStates"></param>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> that can be used to lay out the windows.
	/// </param>
	public WindowDeferPosHandle(
		IContext context,
		IEnumerable<(IWindowState windowState, HWND hwndInsertAfter, SET_WINDOW_POS_FLAGS? flags)> windowStates,
		CancellationToken? cancellationToken = null
	)
		: this(context, cancellationToken)
	{
		_windowStates.AddRange(windowStates);
	}

	/// <summary>
	/// Using the given <paramref name="windowState"/>, sets the window's position.
	/// </summary>
	/// <param name="windowState"></param>
	/// <param name="hwndInsertAfter">The window handle to insert show the given window behind.</param>
	/// <param name="flags">
	/// The flags to use when setting the window position. This overrides the default flags Whim sets,
	/// except when the window is maximized or minimized.
	/// </param>
	public void DeferWindowPos(
		IWindowState windowState,
		HWND? hwndInsertAfter = null,
		SET_WINDOW_POS_FLAGS? flags = null
	)
	{
		Logger.Debug($"Adding window {windowState} after {hwndInsertAfter} with flags {flags}");
		// We use HWND_BOTTOM, as modifying the Z-order of a window
		// may cause EVENT_SYSTEM_FOREGROUND to be set, which in turn
		// causes the relevant window to be focused, when the user hasn't
		// actually changed the focus.
		HWND targetHwndInsertAfter = hwndInsertAfter ?? (HWND)1; // HWND_BOTTOM
		_windowStates.Add((windowState, targetHwndInsertAfter, flags));
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Logger.Debug("Disposing WindowDeferPosHandle");

		if (_cancellationToken?.IsCancellationRequested == true)
		{
			Logger.Debug("Cancellation requested, skipping setting window position");
			return;
		}

		// Check to see if any monitors have non-100% scaling.
		// If so, we need to set the window position twice.
		int numPasses = 1;
		foreach (IMonitor monitor in _context.MonitorManager)
		{
			if (monitor.ScaleFactor != 100)
			{
				numPasses = 2;
				break;
			}
		}

		Logger.Debug($"Setting window position {numPasses} times");

		List<IWindow> toMinimize = new();
		List<IWindow> toMaximize = new();
		List<IWindow> toNormal = new();

		for (int i = 0; i < numPasses; i++)
		{
			Logger.Debug($"Setting window position, pass {i + 1} of {numPasses}");
			if (_cancellationToken?.IsCancellationRequested == true)
			{
				Logger.Debug("Cancellation requested, skipping setting window position");
				return;
			}

			// We don't cancel from here on out, as we're dealing with native code.
			HDWP hdwp = _context.NativeManager.BeginDeferWindowPos(_windowStates.Count);
			foreach ((IWindowState windowState, HWND hwndInsertAfter, SET_WINDOW_POS_FLAGS? flags) in _windowStates)
			{
				hdwp = DeferWindowPos(hdwp, windowState, hwndInsertAfter, flags, toMinimize, toMaximize, toNormal);
			}

			EndDeferWindowPos(hdwp, toMinimize, toMaximize, toNormal);

			// Reset the window states.
			if (i < numPasses - 1)
			{
				toMaximize.Clear();
				toMinimize.Clear();
				toNormal.Clear();
			}
		}
		Logger.Debug("Finished setting window position");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private HDWP DeferWindowPos(
		HDWP hdwp,
		IWindowState windowState,
		HWND hwndInsertAfter,
		SET_WINDOW_POS_FLAGS? flags,
		List<IWindow> toMinimize,
		List<IWindow> toMaximize,
		List<IWindow> toNormal
	)
	{
		IWindow window = windowState.Window;

		ILocation<int>? offset = _context.NativeManager.GetWindowOffset(window.Handle);
		if (offset is null)
		{
			return hdwp;
		}

		ILocation<int> location = windowState.Location.Add(offset);
		WindowSize windowSize = windowState.WindowSize;
		SET_WINDOW_POS_FLAGS uFlags = flags ?? DefaultFlags;

		if (windowSize == WindowSize.Maximized)
		{
			toMaximize.Add(window);
			uFlags = uFlags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}
		else if (windowSize == WindowSize.Minimized)
		{
			toMinimize.Add(window);
			uFlags = uFlags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}
		else
		{
			toNormal.Add(window);
		}

		return _context.NativeManager.DeferWindowPos(
			hdwp,
			window.Handle,
			hwndInsertAfter,
			location.X,
			location.Y,
			location.Width,
			location.Height,
			uFlags
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EndDeferWindowPos(
		HDWP hDWP,
		List<IWindow> toMinimize,
		List<IWindow> toMaximize,
		List<IWindow> toNormal
	)
	{
		foreach (IWindow w in toMinimize)
		{
			if (!w.IsMinimized)
			{
				_context.NativeManager.MinimizeWindow(w.Handle);
			}
		}

		foreach (IWindow w in toMaximize)
		{
			if (!w.IsMaximized)
			{
				_context.NativeManager.ShowWindowMaximized(w.Handle);
			}
		}

		foreach (IWindow w in toNormal)
		{
			_context.NativeManager.ShowWindowNoActivate(w.Handle);
		}

		_context.NativeManager.EndDeferWindowPos(hDWP);
	}
}
