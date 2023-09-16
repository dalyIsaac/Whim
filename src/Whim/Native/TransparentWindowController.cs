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
	private readonly IInternalContext _internalContext;
	private readonly HWND _hwnd;
	private readonly SUBCLASSPROC _subclassProc;
	private bool _disposedValue;

	internal TransparentWindowController(
		IContext context,
		IInternalContext internalContext,
		Microsoft.UI.Xaml.Window window
	)
	{
		_context = context;
		_internalContext = internalContext;
		_hwnd = window.GetHandle();
		ICompositionSupportsSystemBackdrop brushHolder = window.As<ICompositionSupportsSystemBackdrop>();

		_internalContext.CoreNativeManager.EnableBlurBehindWindow(_hwnd);
		brushHolder.SystemBackdrop = _context.NativeManager.Compositor.CreateColorBrush(
			Windows.UI.Color.FromArgb(0, 255, 255, 255)
		);

		_subclassProc = new SUBCLASSPROC(WindowProc);
		_internalContext.CoreNativeManager.SetWindowSubclass(_hwnd, _subclassProc, SUBCLASSID, 0);
	}

	/// <summary>
	/// Listening to these messages is necessary to support transparency when Windows has a light theme.
	/// </summary>
	/// <param name="hWnd"></param>
	/// <param name="uMsg"></param>
	/// <param name="wParam"></param>
	/// <param name="lParam"></param>
	/// <param name="uIdSubclass"></param>
	/// <param name="dwRefData"></param>
	/// <returns></returns>
	private LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
	{
		if (uMsg == PInvoke.WM_ERASEBKGND)
		{
			if (_internalContext.CoreNativeManager.GetClientRect(hWnd, out RECT rect))
			{
				using DeleteObjectSafeHandle brush = _internalContext.CoreNativeManager.CreateSolidBrush(new COLORREF());
				_internalContext.CoreNativeManager.FillRect(new HDC((nint)wParam.Value), rect, brush);
				return (LRESULT)1;
			}
		}
		else if (uMsg == PInvoke.WM_DWMCOMPOSITIONCHANGED)
		{
			_internalContext.CoreNativeManager.EnableBlurBehindWindow(hWnd);
			return (LRESULT)0;
		}

		return _internalContext.CoreNativeManager.DefSubclassProc(hWnd, uMsg, wParam, lParam);
	}

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
			_internalContext.CoreNativeManager.RemoveWindowSubclass(_hwnd, _subclassProc, SUBCLASSID);
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
