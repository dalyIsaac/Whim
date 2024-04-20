using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNext;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace Whim;

internal class WindowManager : IWindowManager, IInternalWindowManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public event EventHandler<WindowEventArgs>? WindowAdded;
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;
	public event EventHandler<WindowEventArgs>? WindowRemoved;
	public event EventHandler<WindowMoveEventArgs>? WindowMoveStart;
	public event EventHandler<WindowMoveEventArgs>? WindowMoved;
	public event EventHandler<WindowMoveEventArgs>? WindowMoveEnd;
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

	/// <summary>
	/// Indicates whether values have been disposed.
	/// </summary>
	private bool _disposedValue;

	public IFilterManager LocationRestoringFilterManager { get; } = new FilterManager();

	internal int WindowMovedDelay { get; init; } = 2000;

	public WindowManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_hookDelegate = new WINEVENTPROC(WinEventProcWrapper);

		DefaultFilteredWindows.LoadLocationRestoringWindows(LocationRestoringFilterManager);
	}

	public void PostInitialize()
	{
		Logger.Debug("Post-initializing window manager...");

		_internalContext.MouseHook.MouseLeftButtonDown += MouseHook_MouseLeftButtonDown;
		_internalContext.MouseHook.MouseLeftButtonUp += MouseHook_MouseLeftButtonUp;
	}

	public IWindow? CreateWindow(HWND hwnd)
	{
		Logger.Verbose($"Adding window {hwnd}");

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
		if (!_context.Store.WindowSlice.Windows.TryGetValue(hwnd, out IWindow? window))
		{
			Logger.Verbose($"Window {hwnd} is not added, event type 0x{eventType:X4}");

			Result<IWindow> windowResult = _context.Store.Dispatch(new AddWindowTransform(hwnd));
			if (!windowResult.TryGet(out window))
			{
				return;
			}
		}

		// TODO

		Logger.Debug($"Windows event 0x{eventType:X4} for {window}");
		switch (eventType)
		{
			// `EVENT_OBJECT_SHOW` is handled by the code above to `AddWindow`.
			case PInvoke.EVENT_OBJECT_SHOW:
				break;
			case PInvoke.EVENT_SYSTEM_FOREGROUND:
			case PInvoke.EVENT_OBJECT_UNCLOAKED:
				_context.Store.Dispatch(new WindowFocusedTransform(window));
				break;
			case PInvoke.EVENT_OBJECT_HIDE:
				OnWindowHidden(window);
				break;
			case PInvoke.EVENT_OBJECT_DESTROY:
			case PInvoke.EVENT_OBJECT_CLOAKED:
				_context.Store.Dispatch(new WindowRemovedTransform(window));
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZESTART:
				_context.Store.Dispatch(new WindowMoveStartedTransform(window));
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZEEND:
				_context.Store.Dispatch(new WindowMoveEndedTransform(window));
				break;
			case PInvoke.EVENT_OBJECT_LOCATIONCHANGE:
				_context.Store.Dispatch(new WindowMovedTransform(window));
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
	/// Handles when a window is hidden.
	/// This will be called when a workspace is deactivated, or when a process hides a window.
	/// For example, Discord will hide its window when it is minimized.
	/// We only care about the hide event if the workspace is active.
	/// </summary>
	/// <param name="window"></param>
	private void OnWindowHidden(IWindow window)
	{
		Logger.Debug($"Window hidden: {window}");

		if (_context.Butler.Pantry.GetMonitorForWindow(window) == null)
		{
			Logger.Debug($"Window {window} is not tracked in a monitor, ignoring event");
			return;
		}

		OnWindowRemoved(window);
	}

	private void MouseHook_MouseLeftButtonDown(object? sender, MouseEventArgs e) => _isLeftMouseButtonDown = true;

	private void MouseHook_MouseLeftButtonUp(object? sender, MouseEventArgs e) => _isLeftMouseButtonDown = false;

	private void OnWindowMinimizeStart(IWindow window)
	{
		Logger.Debug($"Window minimize started: {window}");

		WindowEventArgs args = new() { Window = window };
		_internalContext.ButlerEventHandlers.OnWindowMinimizeStart(args);
		WindowMinimizeStart?.Invoke(this, args);
	}

	private void OnWindowMinimizeEnd(IWindow window)
	{
		Logger.Debug($"Window minimize ended: {window}");

		WindowEventArgs args = new() { Window = window };
		_internalContext.ButlerEventHandlers.OnWindowMinimizeEnd(args);
		WindowMinimizeEnd?.Invoke(this, args);
	}

	public IEnumerator<IWindow> GetEnumerator() => _windows.Values.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
