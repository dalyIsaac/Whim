using System;
using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The sector containing windows.
/// </summary>
internal class WindowSector : SectorBase, IWindowSector, IDisposable, IWindowSectorEvents
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;
	private readonly WindowEventListener _listener;
	private bool _disposedValue;

	/// <summary>
	/// All the windows currently tracked by Whim.
	/// </summary>
	public ImmutableDictionary<HWND, IWindow> Windows { get; internal set; } = ImmutableDictionary<HWND, IWindow>.Empty;

	/// <summary>
	/// The windows which had their first location change event handled - see <see cref="IWindowManager.LocationRestoringFilterManager"/>.
	/// We maintain a set of the windows that have been handled so that we don't enter an infinite loop of location change events.
	/// </summary>
	public ImmutableHashSet<IWindow> HandledLocationRestoringWindows { get; internal set; } =
		ImmutableHashSet<IWindow>.Empty;

	/// <summary>
	/// Whether a window is currently moving.
	/// </summary>
	public bool IsMovingWindow { get; internal set; }

	/// <summary>
	/// Whether the user currently has the left mouse button down.
	/// Used for window movement.
	/// </summary>
	public bool IsLeftMouseButtonDown { get; internal set; }

	/// <summary>
	/// Event for when a window is added by the <see cref="IWindowManager"/>.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowAdded;

	/// <summary>
	/// Event for when a window is focused.
	/// </summary>
	public event EventHandler<WindowFocusedEventArgs>? WindowFocused;

	/// <summary>
	/// Event for when a window is removed from Whim.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowRemoved;

	/// <summary>
	/// Event for when a window is being moved or resized.
	/// </summary>
	public event EventHandler<WindowMoveEventArgs>? WindowMoveStarted;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_SYSTEM_MOVESIZEEND"/> event.
	/// See https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants for more information.
	/// </summary>
	public event EventHandler<WindowMoveEventArgs>? WindowMoveEnded;

	public WindowSector(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
		_listener = new WindowEventListener(ctx, internalCtx);
	}

	// TODO: Add to StoreTests
	public override void Initialize()
	{
		_listener.Initialize();
	}

	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
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
