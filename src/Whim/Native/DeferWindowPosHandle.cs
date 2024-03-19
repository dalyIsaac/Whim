using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Sets the position of multiple windows at once, using <see cref="ICoreNativeManager.SetWindowPos"/>.
/// <br/>
/// This internally calls <see cref="Windows.Win32.PInvoke.SetWindowPos"/> to set the position of the window.
/// Each call occurs in parallel.
/// <br/>
/// When the system has non-100% scaled monitors, for some reason we need to set the window
/// position twice, otherwise windows will have incorrect dimensions.
/// </summary>
public sealed class DeferWindowPosHandle : IDisposable
{
	private static readonly object _lockObj = new();
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	private readonly List<WindowPosState> _windowStates = new();
	private readonly List<WindowPosState> _minimizedWindowStates = new();

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
	/// Create a new <see cref="DeferWindowPosHandle"/> to set the position of multiple windows at once.
	///
	/// <see cref="DeferWindowPosHandle"/> must be used in conjunction with a <c>using</c> block,
	/// otherwise <see cref="ICoreNativeManager.SetWindowPos"/> will not be called.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	internal DeferWindowPosHandle(IContext context, IInternalContext internalContext)
	{
		Logger.Debug("Creating new WindowDeferPosHandle");
		_context = context;
		_internalContext = internalContext;
	}

	/// <summary>
	/// Create a new <see cref="DeferWindowPosHandle"/> to set the position of multiple windows at once.
	///
	/// <see cref="DeferWindowPosHandle"/> must be used in conjunction with a <c>using</c> block,
	/// otherwise <see cref="ICoreNativeManager.SetWindowPos"/> will not be called.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	/// <param name="windowStates"></param>
	internal DeferWindowPosHandle(
		IContext context,
		IInternalContext internalContext,
		IEnumerable<WindowPosState> windowStates
	)
		: this(context, internalContext)
	{
		// Add each window state to the appropriate list.
		foreach (WindowPosState windowState in windowStates)
		{
			AddToCorrectList(windowState);
		}
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
		Logger.Debug($"Adding window {windowState.Window} after {hwndInsertAfter} with flags {flags}");
		// We use HWND_BOTTOM, as modifying the Z-order of a window
		// may cause EVENT_SYSTEM_FOREGROUND to be set, which in turn
		// causes the relevant window to be focused, when the user hasn't
		// actually changed the focus.
		HWND targetHwndInsertAfter = hwndInsertAfter ?? (HWND)1; // HWND_BOTTOM
		AddToCorrectList(new(windowState, targetHwndInsertAfter, flags));
	}

	/// <summary>
	/// Adds the given <paramref name="windowPosState"/> to the appropriate list.
	/// </summary>
	/// <param name="windowPosState">
	/// Minimized windows are added to <see cref="_minimizedWindowStates"/>, otherwise they are added to
	/// <see cref="_windowStates"/>.
	/// </param>
	private void AddToCorrectList(WindowPosState windowPosState)
	{
		if (windowPosState.WindowState.WindowSize == WindowSize.Minimized)
		{
			_minimizedWindowStates.Add(windowPosState);
		}
		else
		{
			_windowStates.Add(windowPosState);
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		using Lock _ = new(_lockObj);
		Logger.Debug("Disposing WindowDeferPosHandle");

		if (_windowStates.Count == 0 && _minimizedWindowStates.Count == 0)
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

		Logger.Debug($"Setting window position {numPasses} times for {_windowStates.Count} windows");

		// Set the window positions for non-minimized windows first, then minimized windows.
		// This was done to prevent the minimized windows being hidden, and Windows focusing the previous window.
		// When windows are restored, then we make sure to focus them - see the window.Focus()` in
		// `Workspace.MinimizeWindowEnd`.
		// This is delicate code - to test:
		// 1. Set monitor 2 to FocusLayoutEngine
		// 2. Focus a tracked window in monitor 1
		// 3. Change focus to a tracked window in monitor 2
		// 4. Focus next window
		// If monitor 1 receives the focus indicator, then this code is broken.
		//
		// This code has worked in the past - however, it relies on `Parallel.ForEach` API calls being ordered,
		// which has no guarantees.
		// However, calling `Parallel.ForEach` separately for minimized windows didn't result in the desired focus
		// behaviour.
		WindowPosState[] allStates = new WindowPosState[_windowStates.Count + _minimizedWindowStates.Count];
		_windowStates.CopyTo(allStates);
		_minimizedWindowStates.CopyTo(allStates, _windowStates.Count);

		if (allStates.Length == 1)
		{
			for (int i = 0; i < numPasses; i++)
			{
				SetWindowPos(allStates[0]);
			}
		}
		else
		{
			for (int i = 0; i < numPasses; i++)
			{
				Parallel.ForEach(allStates, _internalContext.ParallelOptions, SetWindowPos);
			}
		}

		Logger.Debug("Finished setting window position");
	}

	private void SetWindowPos(WindowPosState source)
	{
		IWindow window = source.WindowState.Window;

		IRectangle<int>? offset = _context.NativeManager.GetWindowOffset(window.Handle);
		if (offset is null)
		{
			return;
		}

		IRectangle<int> rect = source.WindowState.Rectangle.Add(offset);
		WindowSize windowSize = source.WindowState.WindowSize;
		SET_WINDOW_POS_FLAGS uFlags = source.Flags ?? DefaultFlags;

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

		Logger.Verbose($"Setting window position for {window} to {rect} with flags {uFlags}");

		_internalContext.CoreNativeManager.SetWindowPos(
			window.Handle,
			source.HwndInsertAfter,
			rect.X,
			rect.Y,
			rect.Width,
			rect.Height,
			uFlags
		);
	}
}
