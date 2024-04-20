using System;
using System.Collections.Generic;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The manager for <see cref="IWindow"/>s.
/// </summary>
public interface IWindowManager : IEnumerable<IWindow>, IDisposable
{
	/// <summary>
	/// Filters for windows that will try restore window locations after their windows are created.
	/// </summary>
	IFilterManager LocationRestoringFilterManager { get; }

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
	Result<IWindow> CreateWindow(HWND hWnd);

	/// <summary>
	/// Event for when a window is added by the <see cref="IWindowManager"/>.
	/// </summary>
	event EventHandler<WindowEventArgs>? WindowAdded;

	/// <summary>
	/// Event for when a window is focused.
	/// </summary>
	event EventHandler<WindowFocusedEventArgs>? WindowFocused;

	/// <summary>
	/// Event for when a window is removed from Whim.
	/// </summary>
	event EventHandler<WindowEventArgs>? WindowRemoved;

	/// <summary>
	/// Event for when a window is being moved or resized.
	/// </summary>
	event EventHandler<WindowMoveEventArgs>? WindowMoveStart;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_OBJECT_LOCATIONCHANGE"/> event.
	/// </summary>
	event EventHandler<WindowMoveEventArgs>? WindowMoved;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends the
	/// <see cref="Windows.Win32.PInvoke.EVENT_SYSTEM_MOVESIZEEND"/> event.
	/// See https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants for more information.
	/// </summary>
	event EventHandler<WindowMoveEventArgs>? WindowMoveEnd;

	/// <summary>
	/// Event for when a window has started being minimized.
	/// </summary>
	event EventHandler<WindowEventArgs>? WindowMinimizeStart;

	/// <summary>
	/// Event for when a window has ended being minimized.
	/// </summary>
	event EventHandler<WindowEventArgs>? WindowMinimizeEnd;
}
