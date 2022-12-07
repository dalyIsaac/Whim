using System;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class WindowManager : IWindowManager
{
	private readonly IConfigContext _configContext;

	public event EventHandler<WindowEventArgs>? WindowAdded;
	public event EventHandler<WindowEventArgs>? WindowFocused;
	public event EventHandler<WindowEventArgs>? WindowRemoved;
	public event EventHandler<WindowEventArgs>? WindowMoveStart;
	public event EventHandler<WindowEventArgs>? WindowMoved;
	public event EventHandler<WindowEventArgs>? WindowMinimizeStart;
	public event EventHandler<WindowEventArgs>? WindowMinimizeEnd;

	/// <summary>
	/// Map of <see cref="HWND"/> to <see cref="IWindow"/> for easy <see cref="IWindow"/> lookup.
	/// </summary>
	private readonly Dictionary<HWND, IWindow> _windows = new();

	/// <summary>
	/// All the hooks added with <see cref="PInvoke.SetWinEventHook(uint, uint, System.Runtime.InteropServices.SafeHandle, WINEVENTPROC, uint, uint, uint)"/>.
	/// </summary>
	private readonly UnhookWinEventSafeHandle[] _addedHooks = new UnhookWinEventSafeHandle[6];

	/// <summary>
	/// The delegate for handling all events triggered by <see cref="PInvoke.SetWinEventHook(uint, uint, System.Runtime.InteropServices.SafeHandle, WINEVENTPROC, uint, uint, uint)"/>.
	/// </summary>
	private readonly WINEVENTPROC _hookDelegate;

	private IWindow? _mouseMoveWindow;

	private readonly object _mouseMoveLock = new();

	/// <summary>
	/// Indicates whether values have been disposed.
	/// </summary>
	private bool _disposedValue;

	public WindowManager(IConfigContext configContext)
	{
		_configContext = configContext;
		_hookDelegate = new WINEVENTPROC(WindowsEventHook);
	}

	public void Initialize()
	{
		Logger.Debug("Initializing window manager...");

		// Each of the following hooks add just one or two event constants from https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants
		_addedHooks[0] = Win32Helper.SetWindowsEventHook(
			PInvoke.EVENT_OBJECT_DESTROY,
			PInvoke.EVENT_OBJECT_HIDE,
			_hookDelegate
		);
		_addedHooks[1] = Win32Helper.SetWindowsEventHook(
			PInvoke.EVENT_OBJECT_CLOAKED,
			PInvoke.EVENT_OBJECT_UNCLOAKED,
			_hookDelegate
		);
		_addedHooks[2] = Win32Helper.SetWindowsEventHook(
			PInvoke.EVENT_SYSTEM_MOVESIZESTART,
			PInvoke.EVENT_SYSTEM_MOVESIZEEND,
			_hookDelegate
		);
		_addedHooks[3] = Win32Helper.SetWindowsEventHook(
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			_hookDelegate
		);
		_addedHooks[4] = Win32Helper.SetWindowsEventHook(
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			_hookDelegate
		);
		_addedHooks[5] = Win32Helper.SetWindowsEventHook(
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
		foreach (HWND hwnd in Win32Helper.GetAllWindows())
		{
			AddWindow(hwnd);
		}
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
	/// <param name="idChild">
	/// Identifies whether the event was triggered by an object or a child element of the object.
	/// If this value is CHILDID_SELF, the event was triggered by the object; otherwise, this value
	/// is the child ID of the element that triggered the event.
	/// </param>
	/// <param name="idObject">
	/// Identifies the object associated with the event. This is one of the object identifiers or a
	/// custom object ID.
	/// </param>
	/// <param name="hwnd">
	/// Handle to the window that generates the event, or NULL if no window is associated with the
	/// event. For example, the mouse pointer is not associated with a window.
	/// </param>
	/// <returns></returns>
	private static bool IsEventWindowValid(int idChild, int idObject, HWND? hwnd) =>
		// When idChild is CHILDID_SELF (0), the event was triggered
		// by the object.
		idChild == PInvoke.CHILDID_SELF
		// When idObject == OBJID_WINDOW (0), the event is
		// associated with the window (not a child object).
		&& idObject == (int)OBJECT_IDENTIFIER.OBJID_WINDOW
		// The handle is not null.
		&& hwnd != null;

	/// <summary>
	/// Event hook for <see cref="PInvoke.SetWinEventHook(uint, uint, System.Runtime.InteropServices.SafeHandle, WINEVENTPROC, uint, uint, uint)"/>. <br />
	///
	/// For more, see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wineventproc
	/// </summary>
	/// <param name="hWinEventHook"></param>
	/// <param name="eventType"></param>
	/// <param name="hwnd"></param>
	/// <param name="idObject"></param>
	/// <param name="idChild"></param>
	/// <param name="idEventThread"></param>
	/// <param name="dwmsEventTime"></param>
	private void WindowsEventHook(
		HWINEVENTHOOK hWinEventHook,
		uint eventType,
		HWND hwnd,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	)
	{
		if (!IsEventWindowValid(idChild, idObject, hwnd))
		{
			return;
		}

		// Try get the window
		if (!_windows.TryGetValue(hwnd, out IWindow? window) || window == null)
		{
			Logger.Verbose($"Window {hwnd.Value} is not added, event type 0x{eventType:X4}");
			window = AddWindow(hwnd);
			if (window == null)
			{
				return;
			}
		}

		Logger.Verbose($"Windows event 0x{eventType:X4} for {window}");
		switch (eventType)
		{
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
	}

	/// <summary>
	/// Add the given <see cref="HWND"/> as an <see cref="IWindow"/> inside this
	/// <see cref="IWindowManager"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	private IWindow? AddWindow(HWND hwnd)
	{
		Logger.Debug($"Adding window {hwnd.Value}");
		if (
			Win32Helper.IsSplashScreen(hwnd)
			|| Win32Helper.IsCloakedWindow(hwnd)
			|| !Win32Helper.IsStandardWindow(hwnd)
			|| !Win32Helper.HasNoVisibleOwner(hwnd)
		)
		{
			return null;
		}

		IWindow? window = IWindow.CreateWindow(_configContext, hwnd);

		if (window == null)
		{
			Logger.Debug($"Window {hwnd.Value} could not be created");
			return null;
		}
		else if (_configContext.FilterManager.ShouldBeIgnored(window))
		{
			Logger.Debug($"Window {window} is filtered");
			return null;
		}
		else if (window.IsMinimized)
		{
			Logger.Debug($"Window {window} is minimized");
			return null;
		}

		// Try add the window to the dictionary.
		if (!_windows.TryAdd(hwnd, window))
		{
			Logger.Debug($"Failed to add {window}");
			return null;
		}

		Logger.Debug($"Added {window}");

		OnWindowAdded(window);
		return window;
	}

	private void OnWindowAdded(IWindow window)
	{
		Logger.Debug($"Window added: {window}");
		(_configContext.WorkspaceManager as WorkspaceManager)?.WindowAdded(window);
		WindowAdded?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	/// <summary>
	/// Handles when the given window is focused.
	/// This can be called by <see cref="Workspace.AddWindow"/>, as an already focused window may
	/// have switched to a different workspace.
	/// </summary>
	/// <param name="window"></param>
	internal void OnWindowFocused(IWindow window)
	{
		Logger.Debug($"Window focused: {window}");
		(_configContext.MonitorManager as MonitorManager)?.WindowFocused(window);
		(_configContext.WorkspaceManager as WorkspaceManager)?.WindowFocused(window);
		WindowFocused?.Invoke(this, new WindowEventArgs() { Window = window });
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

		if (_configContext.WorkspaceManager.GetMonitorForWindow(window) == null)
		{
			Logger.Debug($"Window {window} is not on a monitor, ignoring event");
			return;
		}

		OnWindowRemoved(window);
	}

	private void OnWindowRemoved(IWindow window)
	{
		Logger.Debug($"Window removed: {window}");
		_windows.Remove(window.Handle);
		(_configContext.WorkspaceManager as WorkspaceManager)?.WindowRemoved(window);
		WindowRemoved?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	private void OnWindowMoveStart(IWindow window)
	{
		Logger.Debug($"Window move started: {window}");

		_mouseMoveWindow = window;
		_mouseMoveWindow.IsMouseMoving = true;

		WindowMoveStart?.Invoke(this, new WindowEventArgs() { Window = window });
	}

	private void OnWindowMoveEnd(IWindow window)
	{
		Logger.Debug($"Window move ended: {window}");

		lock (_mouseMoveLock)
		{
			if (_mouseMoveWindow == null)
			{
				return;
			}

			_mouseMoveWindow.IsMouseMoving = false;

			// Move the window.
			if (PInvoke.GetCursorPos(out System.Drawing.Point point))
			{
				_configContext.WorkspaceManager.MoveWindowToPoint(
					window,
					new Point<int>() { X = point.X, Y = point.Y }
				);
			}

			_mouseMoveWindow = null;

			WindowMoved?.Invoke(this, new WindowEventArgs() { Window = window });
		}
	}

	private void OnWindowMoved(IWindow window)
	{
		Logger.Debug($"Window moved: {window}");
		WindowMoved?.Invoke(this, new WindowEventArgs() { Window = window });
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
}
