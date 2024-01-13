using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class WindowManager : IWindowManager, IInternalWindowManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public event EventHandler<WindowEventArgs>? WindowAdded;
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;
	public event EventHandler<WindowEventArgs>? WindowRemoved;
	public event EventHandler<WindowMovedEventArgs>? WindowMoveStart;
	public event EventHandler<WindowMovedEventArgs>? WindowMoved;
	public event EventHandler<WindowMovedEventArgs>? WindowMoveEnd;
	public event EventHandler<WindowEventArgs>? WindowMinimizeStart;
	public event EventHandler<WindowEventArgs>? WindowMinimizeEnd;

	private readonly ConcurrentDictionary<HWND, IWindow> _windows = new();
	public IReadOnlyDictionary<HWND, IWindow> HandleWindowMap => _windows;

	/// <summary>
	/// All the hooks added with <see cref="ICoreNativeManager.SetWinEventHook"/>.
	/// </summary>
	private readonly UnhookWinEventSafeHandle[] _addedHooks = new UnhookWinEventSafeHandle[6];

	/// <summary>
	/// The delegate for handling all events triggered by <see cref="ICoreNativeManager.SetWinEventHook"/>.
	/// </summary>
	private readonly WINEVENTPROC _hookDelegate;

	private bool _isMovingWindow;
	private bool _isLeftMouseButtonDown;
	private readonly object _mouseMoveLock = new();

	/// <summary>
	/// Indicates whether values have been disposed.
	/// </summary>
	private bool _disposedValue;

	public IFilterManager LocationRestoringFilterManager { get; } = new FilterManager();

	/// <summary>
	/// The windows which had their first location change event handled - see <see cref="IWindowManager.LocationRestoringFilterManager"/>.
	/// We maintain a set of the windows that have been handled so that we don't enter an infinite loop of location change events.
	/// </summary>
	private readonly HashSet<IWindow> _handledLocationRestoringWindows = new();

	public WindowManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_hookDelegate = new WINEVENTPROC(WinEventProcWrapper);

		DefaultFilteredWindows.LoadLocationRestoringWindows(LocationRestoringFilterManager);
	}

	public void Initialize()
	{
		Logger.Debug("Initializing window manager...");

		// Each of the following hooks add just one or two event constants from https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants
		_addedHooks[0] = _internalContext.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_OBJECT_DESTROY,
			PInvoke.EVENT_OBJECT_HIDE,
			_hookDelegate
		);
		_addedHooks[1] = _internalContext.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_OBJECT_CLOAKED,
			PInvoke.EVENT_OBJECT_UNCLOAKED,
			_hookDelegate
		);
		_addedHooks[2] = _internalContext.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_SYSTEM_MOVESIZESTART,
			PInvoke.EVENT_SYSTEM_MOVESIZEEND,
			_hookDelegate
		);
		_addedHooks[3] = _internalContext.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			_hookDelegate
		);
		_addedHooks[4] = _internalContext.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			_hookDelegate
		);
		_addedHooks[5] = _internalContext.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_SYSTEM_MINIMIZESTART,
			PInvoke.EVENT_SYSTEM_MINIMIZEEND,
			_hookDelegate
		);

		// If any of the above hooks are invalid, we dispose the WindowManager instance and return false.
		for (int i = 0; i < _addedHooks.Length; i++)
		{
			if (_addedHooks[i].IsInvalid)
			{
				// Disposing is handled by the caller.
				throw new InvalidOperationException($"Failed to add hook {i}");
			}
		}
	}

	public void PostInitialize()
	{
		Logger.Debug("Post-initializing window manager...");

		_internalContext.MouseHook.MouseLeftButtonDown += MouseHook_MouseLeftButtonDown;
		_internalContext.MouseHook.MouseLeftButtonUp += MouseHook_MouseLeftButtonUp;
	}

	public IWindow? CreateWindow(HWND hwnd)
	{
		Logger.Debug($"Adding window {hwnd}");

		if (_windows.TryGetValue(hwnd, out IWindow? window) && window != null)
		{
			Logger.Debug($"Window {hwnd} already exists");
			return window;
		}

		if (Window.CreateWindow(_context, _internalContext, hwnd) is IWindow newWindow)
		{
			Logger.Debug($"Created window {newWindow}");
			return newWindow;
		}

		return null;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing window manager");

				foreach (UnhookWinEventSafeHandle? hook in _addedHooks)
				{
					if (hook == null || hook.IsClosed || hook.IsInvalid)
					{
						continue;
					}

					hook.Dispose();
				}

				_internalContext.MouseHook.MouseLeftButtonDown -= MouseHook_MouseLeftButtonDown;
				_internalContext.MouseHook.MouseLeftButtonUp -= MouseHook_MouseLeftButtonUp;
				_internalContext.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Verifies that the event is for the provided <see cref="HWND"/>. <br/>
	///
	/// Documentation here is based on https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wineventproc
	/// </summary>
	/// <param name="idObject">
	/// Identifies the object associated with the event. This is one of the object identifiers or a
	/// custom object ID.
	/// </param>
	/// <param name="idChild">
	/// Identifies whether the event was triggered by an object or a child element of the object.
	/// If this value is CHILDID_SELF, the event was triggered by the object; otherwise, this value
	/// is the child ID of the element that triggered the event.
	/// </param>
	/// <param name="hwnd">
	/// Handle to the window that generates the event, or NULL if no window is associated with the
	/// event. For example, the mouse pointer is not associated with a window.
	/// </param>
	/// <returns></returns>
	private static bool IsEventWindowValid(int idObject, int idChild, HWND? hwnd) =>
		// When idChild is CHILDID_SELF (0), the event was triggered
		// by the object.
		idChild == PInvoke.CHILDID_SELF
		// When idObject == OBJID_WINDOW (0), the event is
		// associated with the window (not a child object).
		&& idObject == (int)OBJECT_IDENTIFIER.OBJID_WINDOW
		// The handle is not null.
		&& hwnd != null;

	internal void WinEventProcWrapper(
		HWINEVENTHOOK hWinEventHook,
		uint eventType,
		HWND hwnd,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	)
	{
		try
		{
			WinEventProc(hWinEventHook, eventType, hwnd, idObject, idChild, idEventThread, dwmsEventTime);
		}
		catch (Exception e)
		{
			_context.HandleUncaughtException(nameof(WinEventProc), e);
		}
	}

	/// <summary>
	/// Event hook for <see cref="ICoreNativeManager.SetWinEventHook(uint, uint, WINEVENTPROC)"/>. <br />
	///
	/// For more, see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wineventproc
	/// </summary>
	/// <param name="_hWinEventHook"></param>
	/// <param name="eventType"></param>
	/// <param name="hwnd"></param>
	/// <param name="idObject"></param>
	/// <param name="idChild"></param>
	/// <param name="_idEventThread"></param>
	/// <param name="_dwmsEventTime"></param>
	private void WinEventProc(
		HWINEVENTHOOK _hWinEventHook,
		uint eventType,
		HWND hwnd,
		int idObject,
		int idChild,
		uint _idEventThread,
		uint _dwmsEventTime
	)
	{
		if (!IsEventWindowValid(idObject, idChild, hwnd))
		{
			return;
		}

		// Try get the window
		if (!_windows.TryGetValue(hwnd, out IWindow? window) || window == null)
		{
			Logger.Verbose($"Window {hwnd} is not added, event type 0x{eventType:X4}");
			window = AddWindow(hwnd);

			if (
				window == null
				&& (eventType == PInvoke.EVENT_SYSTEM_FOREGROUND || eventType == PInvoke.EVENT_OBJECT_UNCLOAKED)
			)
			{
				// Even if the window was ignored, we need to fire OnWindowFocused.
				Logger.Debug(
					$"Window {hwnd} with event 0x{eventType:X4} was ignored, but still notifying listeners of focus"
				);
				OnWindowFocused(window);

				_internalContext.DeferWindowPosManager.RecoverLayout();
				return;
			}

			if (window == null)
			{
				return;
			}
		}

		Logger.Debug($"Windows event 0x{eventType:X4} for {window}");
		switch (eventType)
		{
			// `EVENT_OBJECT_SHOW` is handled by the code above to `AddWindow`.
			case PInvoke.EVENT_OBJECT_SHOW:
				break;
			case PInvoke.EVENT_SYSTEM_FOREGROUND:
			case PInvoke.EVENT_OBJECT_UNCLOAKED:
				OnWindowFocused(window);
				break;
			case PInvoke.EVENT_OBJECT_HIDE:
				OnWindowHidden(window);
				break;
			case PInvoke.EVENT_OBJECT_DESTROY:
			case PInvoke.EVENT_OBJECT_CLOAKED:
				OnWindowRemoved(window);
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZESTART:
				OnWindowMoveStart(window);
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZEEND:
				OnWindowMoveEnd(window);
				break;
			case PInvoke.EVENT_OBJECT_LOCATIONCHANGE:
				OnWindowMoved(window);
				break;
			case PInvoke.EVENT_SYSTEM_MINIMIZESTART:
				OnWindowMinimizeStart(window);
				break;
			case PInvoke.EVENT_SYSTEM_MINIMIZEEND:
				OnWindowMinimizeEnd(window);
				break;
			default:
				Logger.Error($"Unhandled event 0x{eventType:X4}");
				break;
		}

		_internalContext.DeferWindowPosManager.RecoverLayout();
	}

	public IWindow? AddWindow(HWND hwnd)
	{
		Logger.Debug($"Adding window {hwnd}");

		if (_internalContext.CoreNativeManager.IsSplashScreen(hwnd))
		{
			Logger.Verbose($"Window {hwnd} is a splash screen, ignoring");
			return null;
		}

		if (_internalContext.CoreNativeManager.IsCloakedWindow(hwnd))
		{
			Logger.Verbose($"Window {hwnd} is cloaked, ignoring");
			return null;
		}

		if (!_internalContext.CoreNativeManager.IsStandardWindow(hwnd))
		{
			Logger.Verbose($"Window {hwnd} is not a standard window, ignoring");
			return null;
		}

		if (!_internalContext.CoreNativeManager.HasNoVisibleOwner(hwnd))
		{
			Logger.Verbose($"Window {hwnd} has a visible owner, ignoring");
			return null;
		}

		IWindow? window = CreateWindow(hwnd);
		if (window == null)
		{
			return null;
		}
		if (_context.FilterManager.ShouldBeIgnored(window))
		{
			return null;
		}

		_windows[hwnd] = window;
		OnWindowAdded(window);

		return window;
	}

	public void OnWindowAdded(IWindow window)
	{
		WindowAdded?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	public void OnWindowFocused(IWindow? window)
	{
		Logger.Debug($"Window focused: {window}");
		_internalContext.MonitorManager.WindowFocused(window);
		WindowFocused?.Invoke(this, new WindowFocusedEventArgs() { Window = window });
	}

	/// <summary>
	/// Handles when a window is hidden.
	/// This will be called when a workspace is deactivated, or when a process hides a window.
	/// For example, Discord will hide its window when it is minimized.
	/// We only care about the hide event if the workspace is active.
	/// </summary>
	/// <param name="window"></param>
	private void OnWindowHidden(IWindow window)
	{
		Logger.Debug($"Window hidden: {window}");

		if (_context.WorkspaceManager.GetMonitorForWindow(window) == null)
		{
			Logger.Debug($"Window {window} is not tracked in a monitor, ignoring event");
			return;
		}

		OnWindowRemoved(window);
	}

	public void OnWindowRemoved(IWindow window)
	{
		Logger.Debug($"Window removed: {window}");
		_windows.TryRemove(window.Handle, out _);
		_handledLocationRestoringWindows.Remove(window);
		WindowRemoved?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	private void OnWindowMoveStart(IWindow window)
	{
		Logger.Debug($"Window move started: {window}");
		lock (_mouseMoveLock)
		{
			_isMovingWindow = true;
		}

		IPoint<int>? cursorPoint = null;
		if (_isLeftMouseButtonDown && _internalContext.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		WindowMoveStart?.Invoke(
			this,
			new WindowMovedEventArgs()
			{
				Window = window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = GetMovedEdges(window)?.MovedEdges
			}
		);
	}

	private void MouseHook_MouseLeftButtonDown(object? sender, MouseEventArgs e) => _isLeftMouseButtonDown = true;

	private void MouseHook_MouseLeftButtonUp(object? sender, MouseEventArgs e) => _isLeftMouseButtonDown = false;

	private void OnWindowMoveEnd(IWindow window)
	{
		Logger.Debug($"Window move ended: {window}");

		IPoint<int>? point = null;
		Direction? movedEdges = null;
		lock (_mouseMoveLock)
		{
			if (!_isMovingWindow)
			{
				return;
			}

			if (GetMovedEdges(window) is (Direction MovedEdges, IPoint<int> MovedPoint) moved)
			{
				movedEdges = moved.MovedEdges;
				_context.WorkspaceManager.MoveWindowEdgesInDirection(moved.MovedEdges, moved.MovedPoint, window);
			}
			else if (_internalContext.CoreNativeManager.GetCursorPos(out point))
			{
				_context.WorkspaceManager.MoveWindowToPoint(window, point);
			}

			_isMovingWindow = false;
		}
		WindowMoveEnd?.Invoke(
			this,
			new WindowMovedEventArgs()
			{
				Window = window,
				CursorDraggedPoint = point,
				MovedEdges = movedEdges
			}
		);
	}

	/// <summary>
	/// Tries to move the given window's edges in the direction of the mouse movement.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private (Direction MovedEdges, IPoint<int> MovedPoint)? GetMovedEdges(IWindow window)
	{
		Logger.Debug("Trying to move window edges in direction of mouse movement");
		IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForWindow(window);
		if (workspace is null)
		{
			Logger.Debug($"Could not find workspace for window {window}, failed to move window edges");
			return null;
		}

		IWindowState? windowState = workspace.TryGetWindowState(window);
		if (windowState is null)
		{
			Logger.Debug($"Could not find window state for window {window}, failed to move window edges");
			return null;
		}

		// Get the new window position.
		IRectangle<int>? newRect = _context.NativeManager.DwmGetWindowRectangle(window.Handle);
		if (newRect is null)
		{
			Logger.Debug($"Could not get new rectangle for window {window}, failed to move window edges");
			return null;
		}

		// Find the one or two edges to move.
		int leftEdgeDelta = windowState.Rectangle.X - newRect.X;
		int topEdgeDelta = windowState.Rectangle.Y - newRect.Y;
		int rightEdgeDelta = newRect.X + newRect.Width - (windowState.Rectangle.X + windowState.Rectangle.Width);
		int bottomEdgeDelta = newRect.Y + newRect.Height - (windowState.Rectangle.Y + windowState.Rectangle.Height);

		int movedEdgeCountX = 0;
		int movedEdgeCountY = 0;
		int movedEdgeDeltaX = 0;
		int movedEdgeDeltaY = 0;
		Direction movedEdges = Direction.None;
		if (leftEdgeDelta != 0)
		{
			movedEdges |= Direction.Left;
			movedEdgeDeltaX = -leftEdgeDelta;
			movedEdgeCountX++;
		}
		if (topEdgeDelta != 0)
		{
			movedEdges |= Direction.Up;
			movedEdgeDeltaY = -topEdgeDelta;
			movedEdgeCountY++;
		}
		if (rightEdgeDelta != 0)
		{
			movedEdges |= Direction.Right;
			movedEdgeDeltaX = rightEdgeDelta;
			movedEdgeCountX++;
		}
		if (bottomEdgeDelta != 0)
		{
			movedEdges |= Direction.Down;
			movedEdgeDeltaY = bottomEdgeDelta;
			movedEdgeCountY++;
		}

		if (movedEdgeCountX > 1 || movedEdgeCountY > 1)
		{
			Logger.Debug($"Window {window} moved more than one edge in the same axis, failed to move window edges");
			return null;
		}

		return (movedEdges, new Point<int>() { X = movedEdgeDeltaX, Y = movedEdgeDeltaY, });
	}

	private void OnWindowMoved(IWindow window)
	{
		Logger.Debug($"Window moved: {window}");

		if (!_isMovingWindow)
		{
			if (
				window.ProcessFileName != null
				&& !_handledLocationRestoringWindows.Contains(window)
				&& LocationRestoringFilterManager.ShouldBeIgnored(window)
			)
			{
				// The window's application tried to restore its position.
				// Wait, then restore the position.
				_context.NativeManager.TryEnqueue(async () =>
				{
					await Task.Delay(2000).ConfigureAwait(true);
					if (_context.WorkspaceManager.GetWorkspaceForWindow(window) is IWorkspace workspace)
					{
						_handledLocationRestoringWindows.Add(window);
						workspace.DoLayout();
					}
				});
			}
			else
			{
				// Ignore the window moving event.
				return;
			}
		}

		IPoint<int>? cursorPoint = null;
		if (_isLeftMouseButtonDown && _internalContext.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		WindowMoved?.Invoke(
			this,
			new WindowMovedEventArgs()
			{
				Window = window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = GetMovedEdges(window)?.MovedEdges
			}
		);
	}

	private void OnWindowMinimizeStart(IWindow window)
	{
		Logger.Debug($"Window minimize started: {window}");
		WindowMinimizeStart?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	private void OnWindowMinimizeEnd(IWindow window)
	{
		Logger.Debug($"Window minimize ended: {window}");
		WindowMinimizeEnd?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	public IEnumerator<IWindow> GetEnumerator() => _windows.Values.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
