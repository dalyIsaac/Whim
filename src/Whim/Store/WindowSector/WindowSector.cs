using System;
using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The sector containing windows.
/// </summary>
internal class WindowSector : SectorBase, IWindowSector, IDisposable, IWindowSectorEvents
{
	private readonly WindowEventListener _listener;
	private bool _disposedValue;

	public ImmutableDictionary<HWND, IWindow> Windows { get; internal set; } = ImmutableDictionary<HWND, IWindow>.Empty;

	public ImmutableHashSet<HWND> HandledLocationRestoringWindows { get; internal set; } = ImmutableHashSet<HWND>.Empty;

	public bool IsMovingWindow { get; internal set; }

	public bool IsLeftMouseButtonDown { get; internal set; }

	public int WindowMovedDelay { get; internal set; } = 2000;

	public event EventHandler<WindowAddedEventArgs>? WindowAdded;
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;
	public event EventHandler<WindowRemovedEventArgs>? WindowRemoved;
	public event EventHandler<WindowMoveStartedEventArgs>? WindowMoveStarted;
	public event EventHandler<WindowMoveEndedEventArgs>? WindowMoveEnded;
	public event EventHandler<WindowMovedEventArgs>? WindowMoved;
	public event EventHandler<WindowMinimizeStartedEventArgs>? WindowMinimizeStarted;
	public event EventHandler<WindowMinimizeEndedEventArgs>? WindowMinimizeEnded;

	public WindowSector(IContext ctx, IInternalContext internalCtx)
	{
		_listener = new WindowEventListener(ctx, internalCtx);
	}

	public override void Initialize()
	{
		_listener.Initialize();
	}

	public override void DispatchEvents()
	{
		// Use index access to prevent the list from being modified during enumeration.
		for (int idx = 0; idx < _events.Count; idx++)
		{
			EventArgs eventArgs = _events[idx];
			switch (eventArgs)
			{
				case WindowAddedEventArgs args:
					WindowAdded?.Invoke(this, args);
					break;
				case WindowFocusedEventArgs args:
					WindowFocused?.Invoke(this, args);
					break;
				case WindowRemovedEventArgs args:
					WindowRemoved?.Invoke(this, args);
					break;
				case WindowMoveStartedEventArgs args:
					WindowMoveStarted?.Invoke(this, args);
					break;
				case WindowMoveEndedEventArgs args:
					WindowMoveEnded?.Invoke(this, args);
					break;
				case WindowMovedEventArgs args:
					WindowMoved?.Invoke(this, args);
					break;
				case WindowMinimizeStartedEventArgs args:
					WindowMinimizeStarted?.Invoke(this, args);
					break;
				case WindowMinimizeEndedEventArgs args:
					WindowMinimizeEnded?.Invoke(this, args);
					break;
				default:
					break;
			}
		}

		_events.Clear();
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_listener.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
