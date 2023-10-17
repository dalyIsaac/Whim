using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// The positioning state of a window.
/// </summary>
/// <param name="WindowState"></param>
/// <param name="HwndInsertAfter"></param>
/// <param name="Flags"></param>
public record WindowPosState(IWindowState WindowState, HWND HwndInsertAfter, SET_WINDOW_POS_FLAGS? Flags);

/// <summary>
/// Sets the position of multiple windows at once, using <see cref="INativeManager.SetWindowPos"/>.
/// <br/>
/// This internally calls <see cref="Windows.Win32.PInvoke.SetWindowPos"/> to set the position of the window.
/// Each call occurs in parallel.
/// <br/>
/// When the system has non-100% scaled monitors, for some reason we need to set the window
/// position twice, otherwise windows will have incorrect dimensions.
/// </summary>
public sealed class WindowDeferPosHandle : IDisposable
{
	private readonly IContext _context;
	private readonly List<WindowPosState> _windowStates = new();

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
	/// otherwise <see cref="INativeManager.SetWindowPos"/> will not be called.
	/// </summary>
	/// <param name="context"></param>
	public WindowDeferPosHandle(IContext context)
	{
		Logger.Debug("Creating new WindowDeferPosHandle");
		_context = context;
	}

	/// <summary>
	/// Create a new <see cref="WindowDeferPosHandle"/> to set the position of multiple windows at once.
	///
	/// <see cref="WindowDeferPosHandle"/> must be used in conjunction with a <c>using</c> block,
	/// otherwise <see cref="INativeManager.SetWindowPos"/> will not be called.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="windowStates"></param>
	public WindowDeferPosHandle(IContext context, IEnumerable<WindowPosState> windowStates)
		: this(context)
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
		_windowStates.Add(new(windowState, targetHwndInsertAfter, flags));
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Logger.Debug("Disposing WindowDeferPosHandle");

		if (_windowStates.Count == 0)
		{
			Logger.Debug("No windows to set position for");
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

		IInternalWindowManager internalWindowManager = (IInternalWindowManager)_context.WindowManager;
		if (internalWindowManager.EntriesCount > 1)
		{
			Logger.Debug("Whim has reentered, deferring setting window positions");
			return;
		}

		Logger.Debug($"Setting window position {numPasses} times");
		if (_windowStates.Count == 1)
		{
			for (int i = 0; i < numPasses; i++)
			{
				SetWindowPos(_windowStates[0]);
			}
		}
		else
		{
			for (int i = 0; i < numPasses; i++)
			{
				Parallel.ForEach(_windowStates, (Action<WindowPosState, ParallelLoopState, long>)SetWindowPos);
			}
		}
		Logger.Debug("Finished setting window position");
	}

	private void SetWindowPos(WindowPosState source, ParallelLoopState state, long arg3) => SetWindowPos(source);

	private void SetWindowPos(WindowPosState source)
	{
		(IWindowState windowState, HWND hwndInsertAfter, SET_WINDOW_POS_FLAGS? flags) = source;
		IWindow window = windowState.Window;

		ILocation<int>? offset = _context.NativeManager.GetWindowOffset(window.Handle);
		if (offset is null)
		{
			return;
		}

		ILocation<int> location = windowState.Location.Add(offset);
		WindowSize windowSize = windowState.WindowSize;
		SET_WINDOW_POS_FLAGS uFlags = flags ?? DefaultFlags;

		if (windowSize == WindowSize.Maximized)
		{
			uFlags = uFlags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
			_context.NativeManager.ShowWindowMaximized(window.Handle);
		}
		else if (windowSize == WindowSize.Minimized)
		{
			uFlags = uFlags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
			_context.NativeManager.MinimizeWindow(window.Handle);
		}
		else
		{
			_context.NativeManager.ShowWindowNoActivate(window.Handle);
		}

		_context.NativeManager.SetWindowPos(
			window.Handle,
			hwndInsertAfter,
			location.X,
			location.Y,
			location.Width,
			location.Height,
			uFlags
		);
	}
}
