using Microsoft.UI.Composition;
using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;
using WinRT;

namespace Whim;

/// <summary>
/// Controller for making a window transparent.
/// </summary>
public class TransparentWindowController : IDisposable
{
	private const int SUBCLASSID = 4562;

	private readonly IContext _context;
	private readonly ICoreNativeManager _coreNativeManager;
	private readonly Microsoft.UI.Xaml.Window _window;
	private readonly SUBCLASSPROC _subclassProc;
	private bool _disposedValue;

	internal TransparentWindowController(IContext context, ICoreNativeManager coreNativeManager, Microsoft.UI.Xaml.Window window)
	{
		_context = context;
		_coreNativeManager = coreNativeManager;
		_window = window;

		_coreNativeManager.EnableBlurBehindWindow(_window.GetHandle());
		ICompositionSupportsSystemBackdrop brushHolder = window.As<ICompositionSupportsSystemBackdrop>();
		brushHolder.SystemBackdrop = _context.NativeManager.Compositor.CreateColorBrush(
			Windows.UI.Color.FromArgb(0, 255, 255, 255)
		);

		_subclassProc = new SUBCLASSPROC(WindowProc);
		_coreNativeManager.SetWindowSubclass(_window.GetHandle(), _subclassProc, SUBCLASSID, 0);
	}

	private LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
	{
		if (uMsg == PInvoke.WM_ERASEBKGND)
		{
			if (_coreNativeManager.GetClientRect(hWnd, out RECT rect))
			{
				using DeleteObjectSafeHandle brush = _coreNativeManager.CreateSolidBrush(new COLORREF());
				_coreNativeManager.FillRect(new HDC((nint)wParam.Value), rect, brush);
				return (LRESULT)1;
			}
		}
		else if (uMsg == PInvoke.WM_DWMCOMPOSITIONCHANGED)
		{
			_coreNativeManager.EnableBlurBehindWindow(hWnd);
			return (LRESULT)0;
		}

		return _coreNativeManager.DefSubclassProc(hWnd, uMsg, wParam, lParam);
	}

	/// <inheritdoc />
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
			_disposedValue = true;

			_coreNativeManager.RemoveWindowSubclass(_window.GetHandle(), _subclassProc, SUBCLASSID);
		}
	}

	/// <inheritdoc />
	~TransparentWindowController()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
