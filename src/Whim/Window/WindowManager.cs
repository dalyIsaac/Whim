using System;
using System.Collections;
using System.Collections.Generic;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal class WindowManager : IWindowManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private bool _disposedValue;

	public event EventHandler<WindowAddedEventArgs>? WindowAdded;
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;
	public event EventHandler<WindowRemovedEventArgs>? WindowRemoved;
	public event EventHandler<WindowMoveStartedEventArgs>? WindowMoveStart;
	public event EventHandler<WindowMovedEventArgs>? WindowMoved;
	public event EventHandler<WindowMoveEndedEventArgs>? WindowMoveEnd;
	public event EventHandler<WindowMinimizeStartedEventArgs>? WindowMinimizeStart;
	public event EventHandler<WindowMinimizeEndedEventArgs>? WindowMinimizeEnd;

	public IFilterManager LocationRestoringFilterManager { get; } = new FilterManager();

	public WindowManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;

		DefaultFilteredWindows.LoadLocationRestoringWindows(LocationRestoringFilterManager);
	}

	public void Initialize()
	{
		_context.Store.WindowEvents.WindowAdded += WindowSlice_WindowAdded;
		_context.Store.WindowEvents.WindowFocused += WindowSlice_WindowFocused;
		_context.Store.WindowEvents.WindowRemoved += WindowSlice_WindowRemoved;
		_context.Store.WindowEvents.WindowMoveStarted += WindowSlice_WindowMoveStarted;
		_context.Store.WindowEvents.WindowMoved += WindowSlice_WindowMoved;
		_context.Store.WindowEvents.WindowMoveEnded += WindowSlice_WindowMoveEnd;
		_context.Store.WindowEvents.WindowMinimizeStarted += WindowSlice_WindowMinimizeStarted;
		_context.Store.WindowEvents.WindowMinimizeEnded += WindowSlice_WindowMinimizeEnded;
	}

	private void WindowSlice_WindowAdded(object? sender, WindowAddedEventArgs ev) => WindowAdded?.Invoke(sender, ev);

	private void WindowSlice_WindowFocused(object? sender, WindowFocusedEventArgs ev) =>
		WindowFocused?.Invoke(sender, ev);

	private void WindowSlice_WindowRemoved(object? sender, WindowRemovedEventArgs ev) =>
		WindowRemoved?.Invoke(sender, ev);

	private void WindowSlice_WindowMoveStarted(object? sender, WindowMoveStartedEventArgs ev) =>
		WindowMoveStart?.Invoke(sender, ev);

	private void WindowSlice_WindowMoved(object? sender, WindowMovedEventArgs ev) => WindowMoved?.Invoke(sender, ev);

	private void WindowSlice_WindowMoveEnd(object? sender, WindowMoveEndedEventArgs ev) =>
		WindowMoveEnd?.Invoke(sender, ev);

	private void WindowSlice_WindowMinimizeStarted(object? sender, WindowMinimizeStartedEventArgs ev) =>
		WindowMinimizeStart?.Invoke(sender, ev);

	private void WindowSlice_WindowMinimizeEnded(object? sender, WindowMinimizeEndedEventArgs ev) =>
		WindowMinimizeEnd?.Invoke(sender, ev);

	public Result<IWindow> CreateWindow(HWND hwnd)
	{
		Logger.Verbose($"Adding window {hwnd}");

		Result<IWindow> res = _context.Store.Pick(new TryGetWindowPicker(hwnd));
		if (res.IsSuccessful)
		{
			Logger.Debug($"Window {hwnd} already exists");
			return res;
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
				_context.Store.WindowEvents.WindowAdded -= WindowSlice_WindowAdded;
				_context.Store.WindowEvents.WindowFocused -= WindowSlice_WindowFocused;
				_context.Store.WindowEvents.WindowRemoved -= WindowSlice_WindowRemoved;
				_context.Store.WindowEvents.WindowMoveStarted -= WindowSlice_WindowMoveStarted;
				_context.Store.WindowEvents.WindowMoved -= WindowSlice_WindowMoved;
				_context.Store.WindowEvents.WindowMoveEnded -= WindowSlice_WindowMoveEnd;
				_context.Store.WindowEvents.WindowMinimizeStarted -= WindowSlice_WindowMinimizeStarted;
				_context.Store.WindowEvents.WindowMinimizeEnded -= WindowSlice_WindowMinimizeEnded;
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
