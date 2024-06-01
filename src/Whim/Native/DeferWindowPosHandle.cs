using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	private readonly List<DeferWindowPosState> _windowStates = new();
	private readonly List<DeferWindowPosState> _minimizedWindowStates = new();

	private bool _forceTwoPasses;

	/// <summary>
	/// The default flags to use when setting the window position.
	/// </summary>
	private const SET_WINDOW_POS_FLAGS DefaultFlags =
		SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
		| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
		| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
		| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

	internal static ParallelOptions ParallelOptions { get; set; } = new();

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
		IEnumerable<DeferWindowPosState> windowStates
	)
		: this(context, internalContext)
	{
		// Add each window state to the appropriate list.
		foreach (DeferWindowPosState windowState in windowStates)
		{
			DeferWindowPos(windowState);
		}
	}

	/// <summary>
	/// Adds a window to set the position of when this <see cref="DeferWindowPosHandle"/> disposes.
	/// </summary>
	/// <param name="posState">
	/// The state of the window to set.
	/// </param>
	/// <param name="forceTwoPasses">
	/// Window scaling can be finicky. Whim will set a window's position twice for windows in monitors
	/// with non-100% scaling. Regardless of this, some windows need this even for windows with
	/// 100% scaling.
	/// </param>
	public void DeferWindowPos(DeferWindowPosState posState, bool forceTwoPasses = false)
	{
		Logger.Debug($"Adding window {posState.Handle} after {posState.HandleInsertAfter} with flags {posState.Flags}");

		if (forceTwoPasses)
		{
			_forceTwoPasses = true;
		}

		// We use HWND_BOTTOM, as modifying the Z-order of a window
		// may cause EVENT_SYSTEM_FOREGROUND to be set, which in turn
		// causes the relevant window to be focused, when the user hasn't
		// actually changed the focus.
		if (posState.WindowSize == WindowSize.Minimized)
		{
			_minimizedWindowStates.Add(posState);
		}
		else
		{
			_windowStates.Add(posState);
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
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
			if (monitor.ScaleFactor != 100 || _forceTwoPasses)
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
		DeferWindowPosState[] allStates = new DeferWindowPosState[_windowStates.Count + _minimizedWindowStates.Count];
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
				Parallel.ForEach(allStates, ParallelOptions, SetWindowPos);
			}
		}

		Logger.Debug("Finished setting window position");
	}

	private void SetWindowPos(DeferWindowPosState source)
	{
		IRectangle<int>? offset = _context.NativeManager.GetWindowOffset(source.Handle);
		if (offset is null)
		{
			return;
		}

		IRectangle<int> rect = source.LastWindowRectangle.Add(offset);
		SET_WINDOW_POS_FLAGS uFlags = source.Flags ?? DefaultFlags;

		if (source.WindowSize == WindowSize.Maximized)
		{
			uFlags = uFlags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
			_context.NativeManager.ShowWindowMaximized(source.Handle);
		}
		else if (source.WindowSize == WindowSize.Minimized)
		{
			uFlags = uFlags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
			_context.NativeManager.MinimizeWindow(source.Handle);
		}
		else
		{
			_context.NativeManager.ShowWindowNoActivate(source.Handle);
		}

		Logger.Verbose($"Setting window position for {source.Handle} to {rect} with flags {uFlags}");

		_internalContext.CoreNativeManager.SetWindowPos(
			source.Handle,
			source.HandleInsertAfter,
			rect.X,
			rect.Y,
			rect.Width,
			rect.Height,
			uFlags
		);
	}
}
