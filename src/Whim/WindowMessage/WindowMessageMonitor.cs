using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class WindowMessageMonitor : IWindowMessageMonitor
{
	private const int SUBCLASSID = 4561;

	private readonly IContext _context;
	private readonly ICoreNativeManager _coreNativeManager;
	private readonly SUBCLASSPROC _subclassProc;
	private readonly HWND _windowHwnd;
	private bool _disposedValue;

	public WindowMessageMonitor(IContext context, ICoreNativeManager coreNativeManager)
	{
		_context = context;
		_coreNativeManager = coreNativeManager;

		_windowHwnd = coreNativeManager.CreateWindow();
		_context.NativeManager.HideWindow(_windowHwnd);

		_subclassProc = new SUBCLASSPROC(WindowProc);
		_coreNativeManager.SetWindowSubclass(_windowHwnd, _subclassProc, SUBCLASSID, 0);
		_coreNativeManager.WTSRegisterSessionNotification(_windowHwnd, PInvoke.NOTIFY_FOR_ALL_SESSIONS);
	}

	public event EventHandler<WindowMessageMonitorEventArgs>? DisplayChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? WorkAreaChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? DpiChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? SessionChanged;

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

		return _coreNativeManager.DefSubclassProc(hWnd, uMsg, wParam, lParam);
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
			_coreNativeManager.RemoveWindowSubclass(_windowHwnd, _subclassProc, SUBCLASSID);
			_coreNativeManager.WTSUnRegisterSessionNotification(_windowHwnd);
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
