using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Responsible is responsible for hooking into windows and handling mouse events.
/// </summary>
internal class MouseHook : IMouseHook
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly HOOKPROC _lowLevelMouseProc;
	private UnhookWindowsHookExSafeHandle? _unhookMouseHook;
	private bool disposedValue;
	public event EventHandler<MouseEventArgs>? MouseLeftButtonDown;
	public event EventHandler<MouseEventArgs>? MouseLeftButtonUp;

	public MouseHook(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_lowLevelMouseProc = LowLevelMouseProcWrapper;
	}

	public void PostInitialize()
	{
		Logger.Debug("Initializing mouse manager...");
		_unhookMouseHook = _internalContext.CoreNativeManager.SetWindowsHookEx(
			WINDOWS_HOOK_ID.WH_MOUSE_LL,
			_lowLevelMouseProc,
			null,
			0
		);
	}

	private LRESULT LowLevelMouseProcWrapper(int nCode, WPARAM wParam, LPARAM lParam)
	{
		try
		{
			return LowLevelMouseProc(nCode, wParam, lParam);
		}
		catch (Exception e)
		{
			_context.HandleUncaughtException(nameof(LowLevelMouseProc), e);
			return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
		}
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

		return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
	}

	private void OnMouseTriggerEvent(EventHandler<MouseEventArgs>? eventHandler, LPARAM lParam)
	{
		if (
			lParam != 0
			&& eventHandler is EventHandler<MouseEventArgs> handler
			&& _internalContext.CoreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(lParam)
				is MSLLHOOKSTRUCT mouseHookStruct
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
