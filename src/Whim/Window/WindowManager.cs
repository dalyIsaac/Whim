using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal class WindowManager : IWindowManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private bool _disposedValue;

	public event EventHandler<WindowEventArgs>? WindowAdded;
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;
	public event EventHandler<WindowEventArgs>? WindowRemoved;
	public event EventHandler<WindowMoveEventArgs>? WindowMoveStart;
	public event EventHandler<WindowMoveEventArgs>? WindowMoved;
	public event EventHandler<WindowMoveEventArgs>? WindowMoveEnd;
	public event EventHandler<WindowEventArgs>? WindowMinimizeStart;
	public event EventHandler<WindowEventArgs>? WindowMinimizeEnd;

	public IFilterManager LocationRestoringFilterManager { get; } = new FilterManager();

	internal int WindowMovedDelay { get; init; } = 2000;

	public WindowManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;

		DefaultFilteredWindows.LoadLocationRestoringWindows(LocationRestoringFilterManager);
	}

	public void Initialize()
	{
		_context.Store.WindowSlice.WindowAdded += WindowSlice_WindowAdded;
		_context.Store.WindowSlice.WindowFocused += WindowSlice_WindowFocused;
		_context.Store.WindowSlice.WindowRemoved += WindowSlice_WindowRemoved;
		_context.Store.WindowSlice.WindowMoveStarted += WindowSlice_WindowMoveStarted;
		_context.Store.WindowSlice.WindowMoved += WindowSlice_WindowMoved;
		_context.Store.WindowSlice.WindowMoveEnded += WindowSlice_WindowMoveEnd;
		_context.Store.WindowSlice.WindowMinimizeStarted += WindowSlice_WindowMinimizeStarted;
		_context.Store.WindowSlice.WindowMinimizeEnded += WindowSlice_WindowMinimizeEnded;
	}

	public void WindowSlice_WindowAdded(object? sender, WindowAddedEventArgs ev) => WindowAdded?.Invoke(sender, ev);

	public void WindowSlice_WindowFocused(object? sender, WindowFocusedEventArgs ev) =>
		WindowFocused?.Invoke(sender, ev);

	public void WindowSlice_WindowRemoved(object? sender, WindowRemovedEventArgs ev) =>
		WindowRemoved?.Invoke(sender, ev);

	public void WindowSlice_WindowMoveStarted(object? sender, WindowMoveStartedEventArgs ev) =>
		WindowMoveStart?.Invoke(sender, ev);

	public void WindowSlice_WindowMoved(object? sender, WindowMovedStartedEventArgs ev) =>
		WindowMoved?.Invoke(sender, ev);

	public void WindowSlice_WindowMoveEnd(object? sender, WindowMoveEndedEventArgs ev) =>
		WindowMoveEnd?.Invoke(sender, ev);

	public void WindowSlice_WindowMinimizeStarted(object? sender, WindowMinimizeStartedEventArgs ev) =>
		WindowMinimizeStart?.Invoke(sender, ev);

	public void WindowSlice_WindowMinimizeEnded(object? sender, WindowMinimizeEndedEventArgs ev) =>
		WindowMinimizeEnd?.Invoke(sender, ev);

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

	public IEnumerator<IWindow> GetEnumerator() => _context.Store.Pick(new GetAllWindowsPicker()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.Store.WindowSlice.WindowAdded -= WindowSlice_WindowAdded;
				_context.Store.WindowSlice.WindowFocused -= WindowSlice_WindowFocused;
				_context.Store.WindowSlice.WindowRemoved -= WindowSlice_WindowRemoved;
				_context.Store.WindowSlice.WindowMoveStarted -= WindowSlice_WindowMoveStarted;
				_context.Store.WindowSlice.WindowMoved -= WindowSlice_WindowMoved;
				_context.Store.WindowSlice.WindowMoveEnded -= WindowSlice_WindowMoveEnd;
				_context.Store.WindowSlice.WindowMinimizeStarted -= WindowSlice_WindowMinimizeStarted;
				_context.Store.WindowSlice.WindowMinimizeEnded -= WindowSlice_WindowMinimizeEnded;
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
