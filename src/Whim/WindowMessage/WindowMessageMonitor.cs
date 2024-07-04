using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class WindowMessageMonitor : IWindowMessageMonitor
{
	private const int SUBCLASSID = 4561;

	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly SUBCLASSPROC _subclassProc;
	private bool _disposedValue;

	public WindowMessageMonitor(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_subclassProc = new SUBCLASSPROC(WindowProcWrapper);
	}

	public event EventHandler<WindowMessageMonitorEventArgs>? DisplayChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? WorkAreaChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? DpiChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? SessionChanged;

	public void PreInitialize()
	{
		_context.NativeManager.HideWindow(_internalContext.CoreNativeManager.WindowMessageMonitorWindowHandle);

		_internalContext.CoreNativeManager.SetWindowSubclass(
			_internalContext.CoreNativeManager.WindowMessageMonitorWindowHandle,
			_subclassProc,
			SUBCLASSID,
			0
		);
		_internalContext.CoreNativeManager.WTSRegisterSessionNotification(
			_internalContext.CoreNativeManager.WindowMessageMonitorWindowHandle,
			PInvoke.NOTIFY_FOR_ALL_SESSIONS
		);
	}

	private LRESULT WindowProcWrapper(
		HWND hWnd,
		uint uMsg,
		WPARAM wParam,
		LPARAM lParam,
		nuint uIdSubclass,
		nuint dwRefData
	)
	{
		try
		{
			return WindowProc(hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData);
		}
		catch (Exception e)
		{
			_context.HandleUncaughtException(nameof(WindowProc), e);
			return _internalContext.CoreNativeManager.DefSubclassProc(hWnd, uMsg, wParam, lParam);
		}
	}

	private LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
	{
		WindowMessageMonitorEventArgs eventArgs =
			new()
			{
				MessagePayload = new()
				{
					HWnd = hWnd,
					UMsg = uMsg,
					WParam = wParam,
					LParam = lParam,
				}
			};

		switch (uMsg)
		{
			case PInvoke.WM_DISPLAYCHANGE:
				Logger.Debug("Display changed");
				DisplayChanged?.Invoke(this, eventArgs);
				break;
			case PInvoke.WM_SETTINGCHANGE:
				WindowProcSettingChange(eventArgs);
				break;
			case PInvoke.WM_WTSSESSION_CHANGE:
				Logger.Debug("Session changed");
				SessionChanged?.Invoke(this, eventArgs);
				break;
			default:
				break;
		}

		if (eventArgs.Handled)
		{
			return new LRESULT(eventArgs.Result);
		}

		return _internalContext.CoreNativeManager.DefSubclassProc(hWnd, uMsg, wParam, lParam);
	}

	private void WindowProcSettingChange(WindowMessageMonitorEventArgs eventArgs)
	{
		switch (eventArgs.MessagePayload.WParam)
		{
			case (nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA:
				Logger.Debug("Work area changed");
				WorkAreaChanged?.Invoke(this, eventArgs);
				break;
			case (nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETLOGICALDPIOVERRIDE:
				Logger.Debug("DPI changed");
				DpiChanged?.Invoke(this, eventArgs);
				break;
			default:
				break;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_internalContext.CoreNativeManager.RemoveWindowSubclass(
				_internalContext.CoreNativeManager.WindowMessageMonitorWindowHandle,
				_subclassProc,
				SUBCLASSID
			);
			_internalContext.CoreNativeManager.WTSUnRegisterSessionNotification(
				_internalContext.CoreNativeManager.WindowMessageMonitorWindowHandle
			);
			_disposedValue = true;
		}
	}

	// Free unmanaged resources
	~WindowMessageMonitor()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
