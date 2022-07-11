using System;

namespace Whim;

/// <summary>
/// The manager for <see cref="IWindow"/>s.
/// </summary>
public interface IWindowManager : IDisposable
{
	/// <summary>
	/// Initialize the windows event hooks.
	/// </summary>
	/// <returns></returns>
	public void Initialize();

	/// <summary>
	/// Register the top-level windows.
	/// </summary>
	public void PostInitialize();

	/// <summary>
	/// Event for when a window is registered by the <see cref="IWindowManager"/>.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowRegistered;

	/// <summary>
	/// Event for when a window is focused.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowFocused;

	/// <summary>
	/// Event for when a window is unregistered from Whim.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowUnregistered;

	/// <summary>
	/// Event for when a window is being moved or resized.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowMoveStart;

	/// <summary>
	/// Event for when a window has changed location, shape, or size.
	///
	/// This event is fired when Windows sends a <see cref="Windows.Win32.PInvoke.EVENT_OBJECT_LOCATIONCHANGE"/>
	/// or <see cref="Windows.Win32.PInvoke.EVENT_SYSTEM_MOVESIZEEND"/> event.
	/// See https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants for more information.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowMoved;

	/// <summary>
	/// Event for when a window has started being minimized.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowMinimizeStart;

	/// <summary>
	/// Event for when a window has ended being minimized.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowMinimizeEnd;
}
