namespace Whim;

/// <summary>
/// The manager for <see cref="IWindow"/>s.
/// </summary>
[Obsolete("Use transforms and pickers to interact with the store instead.")]
public interface IWindowManager : IEnumerable<IWindow>, IDisposable
{
	/// <summary>
	/// Initialize the windows event hooks.
	/// </summary>
	/// <returns></returns>
	void Initialize();

	/// <summary>
	/// Creates a new window. If the window cannot be created, <see langword="null"/> is returned.
	/// This will try reuse existing <see cref="IWindow"/>s if possible.
	/// </summary>
	/// <remarks>
	/// This does not add the window to the <see cref="IWindowManager"/> or to the <see cref="IWorkspaceManager"/>.
	/// </remarks>
	/// <param name="hWnd">The window handle.</param>
	/// <returns></returns>
	[Obsolete("Use PickWindowByHandle and Window.CreateWindow instead.")]
	Result<IWindow> CreateWindow(HWND hWnd);

	/// <summary>
	/// Event for when a window is added by the <see cref="IWindowManager"/>.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowAdded event instead.")]
	event EventHandler<WindowAddedEventArgs>? WindowAdded;

	/// <summary>
	/// Event for when a window is focused.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowFocused event instead.")]
	event EventHandler<WindowFocusedEventArgs>? WindowFocused;

	/// <summary>
	/// Event for when a window is removed from Whim.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowRemoved event instead.")]
	event EventHandler<WindowRemovedEventArgs>? WindowRemoved;

	/// <summary>
	/// Event for when a window is being moved or resized.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowMoveStarted event instead.")]
	event EventHandler<WindowMoveStartedEventArgs>? WindowMoveStart;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_OBJECT_LOCATIONCHANGE"/> event.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowMoved event instead.")]
	event EventHandler<WindowMovedEventArgs>? WindowMoved;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_SYSTEM_MOVESIZEEND"/> event.
	/// See https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants for more information.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowMoveEnded event instead.")]
	event EventHandler<WindowMoveEndedEventArgs>? WindowMoveEnd;

	/// <summary>
	/// Event for when a window has started being minimized.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowMinimizeStarted event instead.")]
	event EventHandler<WindowMinimizeStartedEventArgs>? WindowMinimizeStart;

	/// <summary>
	/// Event for when a window has ended being minimized.
	/// </summary>
	[Obsolete("Use the IStore.IWindowSectorEvents.WindowMinimizeEnded event instead.")]
	event EventHandler<WindowMinimizeEndedEventArgs>? WindowMinimizeEnd;
}
