using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Responsible is responsible for hooking into windows and handling mouse events.
/// </summary>
internal class MouseHook : IMouseHook, IDisposable
{
	private readonly ICoreNativeManager _coreNativeManager;
	private readonly HOOKPROC _lowLevelMouseProc;
	private UnhookWindowsHookExSafeHandle? _unhookMouseHook;
	private bool disposedValue;
	public event EventHandler<MouseEventArgs>? MouseLeftButtonDown;
	public event EventHandler<MouseEventArgs>? MouseLeftButtonUp;

	public MouseHook(ICoreNativeManager coreNativeManager)
	{
		_coreNativeManager = coreNativeManager;
		_lowLevelMouseProc = LowLevelMouseProc;
	}

	public void PostInitialize()
	{
		Logger.Debug("Initializing mouse manager...");
		_unhookMouseHook = _coreNativeManager.SetWindowsHookEx(
			WINDOWS_HOOK_ID.WH_MOUSE_LL,
			_lowLevelMouseProc,
			null,
			0
		);
	}

	/// <summary>
	/// For relevant documentation, see https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelmouseproc
	/// </summary>
	/// <param name="nCode"></param>
	/// <param name="wParam"></param>
	/// <param name="lParam"></param>
	/// <returns></returns>
	private LRESULT LowLevelMouseProc(int nCode, WPARAM wParam, LPARAM lParam)
	{
		switch ((uint)wParam)
		{
			case PInvoke.WM_LBUTTONDOWN:
				OnMouseTriggerEvent(MouseLeftButtonDown, lParam);
				break;
			case PInvoke.WM_LBUTTONUP:
				OnMouseTriggerEvent(MouseLeftButtonUp, lParam);
				break;
			default:
				break;
		}

		return _coreNativeManager.CallNextHookEx(nCode, wParam, lParam);
	}

	private void OnMouseTriggerEvent(EventHandler<MouseEventArgs>? eventHandler, LPARAM lParam)
	{
		if (
			lParam != 0
			&& eventHandler is EventHandler<MouseEventArgs> handler
			&& _coreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(lParam) is MSLLHOOKSTRUCT mouseHookStruct
		)
		{
			handler.Invoke(this, new MouseEventArgs(new Point<int>(mouseHookStruct.pt.X, mouseHookStruct.pt.Y)));
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_unhookMouseHook?.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
