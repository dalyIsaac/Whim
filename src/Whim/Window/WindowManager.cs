using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal class WindowManager : IWindowManager
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

	public IFilterManager LocationRestoringFilterManager { get; } = new FilterManager();

	internal int WindowMovedDelay { get; init; } = 2000;

	public WindowManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;

		DefaultFilteredWindows.LoadLocationRestoringWindows(LocationRestoringFilterManager);
	}

	public Result<IWindow> CreateWindow(HWND hwnd)
	{
		Logger.Verbose($"Adding window {hwnd}");

		if (_windows.TryGetValue(hwnd, out IWindow? window) && window != null)
		{
			Logger.Debug($"Window {hwnd} already exists");
			return Result.FromValue(window);
		}

		return Window.CreateWindow(_context, _internalContext, hwnd);
	}

	public IEnumerator<IWindow> GetEnumerator() => _windows.Values.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
