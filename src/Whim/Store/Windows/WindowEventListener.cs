using System;
using DotNext;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class WindowEventListener : IDisposable
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;
	private bool _disposedValue;

	/// <summary>
	/// All the hooks added with <see cref="ICoreNativeManager.SetWinEventHook"/>.
	/// </summary>
	private readonly UnhookWinEventSafeHandle[] _addedHooks = new UnhookWinEventSafeHandle[6];

	/// <summary>
	/// The delegate for handling all events triggered by <see cref="ICoreNativeManager.SetWinEventHook"/>.
	/// </summary>
	private readonly WINEVENTPROC _hookDelegate;

	public WindowEventListener(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
		_hookDelegate = new WINEVENTPROC(WinEventProcWrapper);
	}

	public void Initialize()
	{
		Logger.Debug("Initializing window manager...");

		// Each of the following hooks add just one or two event constants from https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants
		_addedHooks[0] = _internalCtx.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_OBJECT_DESTROY,
			PInvoke.EVENT_OBJECT_HIDE,
			_hookDelegate
		);
		_addedHooks[1] = _internalCtx.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_OBJECT_CLOAKED,
			PInvoke.EVENT_OBJECT_UNCLOAKED,
			_hookDelegate
		);
		_addedHooks[2] = _internalCtx.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_SYSTEM_MOVESIZESTART,
			PInvoke.EVENT_SYSTEM_MOVESIZEEND,
			_hookDelegate
		);
		_addedHooks[3] = _internalCtx.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			_hookDelegate
		);
		_addedHooks[4] = _internalCtx.CoreNativeManager.SetWinEventHook(
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			_hookDelegate
		);
		_addedHooks[5] = _internalCtx.CoreNativeManager.SetWinEventHook(
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

	private void WinEventProcWrapper(
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
			// TODO
			WinEventProc(hWinEventHook, eventType, hwnd, idObject, idChild, idEventThread, dwmsEventTime);
		}
		catch (Exception e)
		{
			_ctx.HandleUncaughtException(nameof(WinEventProc), e);
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
		if (!_ctx.Store.WindowSlice.Windows.TryGetValue(hwnd, out IWindow? window))
		{
			Logger.Verbose($"Window {hwnd} is not added, event type 0x{eventType:X4}");

			Result<IWindow> windowResult = _ctx.Store.Dispatch(new AddWindowTransform(hwnd));
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
				_ctx.Store.Dispatch(new WindowFocusedTransform(window));
				break;
			case PInvoke.EVENT_OBJECT_HIDE:
				_ctx.Store.Dispatch(new WindowHiddenTransform(window));
				break;
			case PInvoke.EVENT_OBJECT_DESTROY:
			case PInvoke.EVENT_OBJECT_CLOAKED:
				_ctx.Store.Dispatch(new WindowRemovedTransform(window));
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZESTART:
				_ctx.Store.Dispatch(new WindowMoveStartedTransform(window));
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZEEND:
				_ctx.Store.Dispatch(new WindowMoveEndedTransform(window));
				break;
			case PInvoke.EVENT_OBJECT_LOCATIONCHANGE:
				_ctx.Store.Dispatch(new WindowMovedTransform(window));
				break;
			case PInvoke.EVENT_SYSTEM_MINIMIZESTART:
				_ctx.Store.Dispatch(new WindowMinimizeStartedTransform(window));
				break;
			case PInvoke.EVENT_SYSTEM_MINIMIZEEND:
				_ctx.Store.Dispatch(new WindowMinimizeEndedTransform(window));
				break;
			default:
				Logger.Error($"Unhandled event 0x{eventType:X4}");
				break;
		}
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

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
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
		GC.SuppressFinalize(this);
	}
}
