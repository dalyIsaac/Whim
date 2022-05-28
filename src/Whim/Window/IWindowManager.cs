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
	/// Event for when a window is registered by the <see cref="IWindowManager"/>.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowRegistered;

	/// <summary>
	/// Event for when a window is updated.
	/// </summary>
	public event EventHandler<WindowUpdateEventArgs>? WindowUpdated;

	/// <summary>
	/// Event for when a window is focused.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowFocused;

	/// <summary>
	/// Event for when a window is unregistered.
	/// </summary>
	public event EventHandler<WindowEventArgs>? WindowUnregistered;

	/// <summary>
	/// Used by <see cref="IWindow"/> to trigger <see cref="WindowUpdated"/>.
	/// </summary>
	public void TriggerWindowUpdated(WindowUpdateEventArgs args);

	/// <summary>
	/// Used by <see cref="IWindow"/> to trigger <see cref="WindowFocused"/>.
	/// </summary>
	public void TriggerWindowFocused(WindowEventArgs args);

	/// <summary>
	/// Used by <see cref="IWindow"/> to trigger <see cref="WindowUnregistered"/>.
	/// </summary>
	public void TriggerWindowUnregistered(WindowEventArgs args);
}
