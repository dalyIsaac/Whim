using System.Collections;

namespace Whim;

internal class WindowManager(IContext context) : IWindowManager, IInternalWindowManager
{
	private readonly IContext _context = context;

	public event EventHandler<WindowAddedEventArgs>? WindowAdded;
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;
	public event EventHandler<WindowRemovedEventArgs>? WindowRemoved;
	public event EventHandler<WindowMoveStartedEventArgs>? WindowMoveStart;
	public event EventHandler<WindowMovedEventArgs>? WindowMoved;
	public event EventHandler<WindowMoveEndedEventArgs>? WindowMoveEnd;
	public event EventHandler<WindowMinimizeStartedEventArgs>? WindowMinimizeStart;
	public event EventHandler<WindowMinimizeEndedEventArgs>? WindowMinimizeEnd;

	/// <summary>
	/// Indicates whether values have been disposed.
	/// </summary>
	private bool _disposedValue;

	public void Initialize()
	{
		_context.Store.WindowEvents.WindowAdded += WindowSector_WindowAdded;
		_context.Store.WindowEvents.WindowFocused += WindowSector_WindowFocused;
		_context.Store.WindowEvents.WindowRemoved += WindowSector_WindowRemoved;
		_context.Store.WindowEvents.WindowMoveStarted += WindowSector_WindowMoveStarted;
		_context.Store.WindowEvents.WindowMoved += WindowSector_WindowMoved;
		_context.Store.WindowEvents.WindowMoveEnded += WindowSector_WindowMoveEnd;
		_context.Store.WindowEvents.WindowMinimizeStarted += WindowSector_WindowMinimizeStarted;
		_context.Store.WindowEvents.WindowMinimizeEnded += WindowSector_WindowMinimizeEnded;
	}

	private void WindowSector_WindowAdded(object? sender, WindowAddedEventArgs ev) => WindowAdded?.Invoke(sender, ev);

	private void WindowSector_WindowFocused(object? sender, WindowFocusedEventArgs ev) =>
		WindowFocused?.Invoke(sender, ev);

	private void WindowSector_WindowRemoved(object? sender, WindowRemovedEventArgs ev) =>
		WindowRemoved?.Invoke(sender, ev);

	private void WindowSector_WindowMoveStarted(object? sender, WindowMoveStartedEventArgs ev) =>
		WindowMoveStart?.Invoke(sender, ev);

	private void WindowSector_WindowMoved(object? sender, WindowMovedEventArgs ev) => WindowMoved?.Invoke(sender, ev);

	private void WindowSector_WindowMoveEnd(object? sender, WindowMoveEndedEventArgs ev) =>
		WindowMoveEnd?.Invoke(sender, ev);

	private void WindowSector_WindowMinimizeStarted(object? sender, WindowMinimizeStartedEventArgs ev) =>
		WindowMinimizeStart?.Invoke(sender, ev);

	private void WindowSector_WindowMinimizeEnded(object? sender, WindowMinimizeEndedEventArgs ev) =>
		WindowMinimizeEnd?.Invoke(sender, ev);

	public Result<IWindow> CreateWindow(HWND hwnd) => _context.CreateWindow(hwnd);

	public IWindow? AddWindow(HWND hwnd) => _context.Store.Dispatch(new WindowAddedTransform(hwnd)).ValueOrDefault;

	public void OnWindowFocused(IWindow? window) => _context.Store.Dispatch(new WindowFocusedTransform(window));

	public void OnWindowRemoved(IWindow window) => _context.Store.Dispatch(new WindowRemovedTransform(window));

	public IEnumerator<IWindow> GetEnumerator() => _context.Store.Pick(PickAllWindows()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.Store.WindowEvents.WindowAdded -= WindowSector_WindowAdded;
				_context.Store.WindowEvents.WindowFocused -= WindowSector_WindowFocused;
				_context.Store.WindowEvents.WindowRemoved -= WindowSector_WindowRemoved;
				_context.Store.WindowEvents.WindowMoveStarted -= WindowSector_WindowMoveStarted;
				_context.Store.WindowEvents.WindowMoved -= WindowSector_WindowMoved;
				_context.Store.WindowEvents.WindowMoveEnded -= WindowSector_WindowMoveEnd;
				_context.Store.WindowEvents.WindowMinimizeStarted -= WindowSector_WindowMinimizeStarted;
				_context.Store.WindowEvents.WindowMinimizeEnded -= WindowSector_WindowMinimizeEnded;
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
		System.GC.SuppressFinalize(this);
	}
}
