namespace Whim;

internal interface IInternalWorkspaceManager
{
	/// <summary>
	/// Called when a window has been added by the <see cref="IWindowManager"/>.
	/// </summary>
	/// <param name="window">The window that was added.</param>
	void WindowAdded(IWindow window);

	/// <summary>
	/// Called when a window has been removed by the <see cref="IWindowManager"/>.
	/// </summary>
	/// <param name="window">The window that was removed.</param>
	void WindowRemoved(IWindow window);

	/// <summary>
	/// Called when a window has been focused by the <see cref="IWindowManager"/>.
	/// </summary>
	/// <param name="window">The window that was focused.</param>
	void WindowFocused(IWindow? window);

	/// <summary>
	/// Called when a window is about to be minimized.
	/// </summary>
	/// <param name="window">The window that is minimizing.</param>
	void WindowMinimizeStart(IWindow window);

	/// <summary>
	/// Called when a window is about to be restored.
	/// </summary>
	/// <param name="window">The window that is restoring.</param>
	void WindowMinimizeEnd(IWindow window);
}
