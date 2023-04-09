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
	private readonly Microsoft.UI.Xaml.Window _window;
	private bool _disposedValue;

	public WindowMessageMonitor(IContext context, ICoreNativeManager coreNativeManager)
	{
		_context = context;
		_coreNativeManager = coreNativeManager;

		_window = new();
		_window.SetIsShownInSwitchers(false);

		HWND hwnd = _window.GetHandle();
		_context.NativeManager.HideWindow(hwnd);

		_subclassProc = new SUBCLASSPROC(WindowProc);
		_coreNativeManager.SetWindowSubclass(new HWND(hwnd), _subclassProc, SUBCLASSID, 0);
	}

	public event EventHandler<WindowMessageMonitorEventArgs>? DisplayChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? WorkAreaChanged;

	public event EventHandler<WindowMessageMonitorEventArgs>? DpiChanged;

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
				DisplayChanged?.Invoke(this, eventArgs);
				break;
			case PInvoke.WM_SETTINGCHANGE:
				WindowProcSettingChange(eventArgs);
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
				WorkAreaChanged?.Invoke(this, eventArgs);
				break;
			case (nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETLOGICALDPIOVERRIDE:
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
			_coreNativeManager.RemoveWindowSubclass(_window.GetHandle(), _subclassProc, SUBCLASSID);
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
