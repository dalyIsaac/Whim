using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

public class WindowDeferPosHandle : IWindowDeferPosHandle
{
	private nint _hWinPosInfo;
	private readonly List<IWindow> _toMinimize;
	private readonly List<IWindow> _toMaximize;
	private readonly List<IWindow> _toNormal;

	public WindowDeferPosHandle(nint info)
	{
		_hWinPosInfo = info;
		_toMinimize = new List<IWindow>();
		_toMaximize = new List<IWindow>();
		_toNormal = new List<IWindow>();
	}

	public void Dispose()
	{
		foreach (IWindow w in _toMinimize)
		{
			if (!w.IsMinimized)
			{
				Win32Helper.MinimizeWindow(w.Handle);
			}
		}
		foreach (IWindow w in _toMaximize)
		{
			if (!w.IsMaximized)
			{
				Win32Helper.ShowWindowMaximized(w.Handle);
			}
		}
		foreach (IWindow w in _toNormal)
		{
			Win32Helper.ShowWindowNoActivate(w.Handle);
		}

		PInvoke.EndDeferWindowPos(_hWinPosInfo);
	}

	public void DeferWindowPos(IWindowLocation windowLocation)
	{
		IWindow window = windowLocation.Window;
		ILocation location = windowLocation.Location;
		WindowState windowState = windowLocation.WindowState;

		SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
			| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
			| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
			| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
			| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

		if (windowState == WindowState.Maximized)
		{
			_toMaximize.Add(window);
			flags = flags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}
		else if (windowState == WindowState.Minimized)
		{
			_toMinimize.Add(window);
			flags = flags | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}
		else
		{
			_toNormal.Add(window);
		}

		_hWinPosInfo = PInvoke.DeferWindowPos(_hWinPosInfo,
										window.Handle,
										new Windows.Win32.Foundation.HWND(0), // HWND_TOP
										location.X,
										location.Y,
										location.Width,
										location.Height,
										flags);
	}

	public static IWindowDeferPosHandle Initialize(int count)
	{
		nint info = PInvoke.BeginDeferWindowPos(count);
		return new WindowDeferPosHandle(info);
	}
}
