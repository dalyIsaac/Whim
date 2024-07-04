namespace Whim;

internal interface IInternalWindowManager
{
	/// <summary>
	/// Add the given <see cref="HWND"/> as an <see cref="IWindow"/> inside this
	/// <see cref="IWindowManager"/>.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <returns>The newly added <see cref="IWindow"/>, or <see langword="null"/> if the <see cref="HWND"/> was ignored.</returns>
	IWindow? AddWindow(HWND hwnd);

	/// <summary>
	/// Handles when the given window is focused.
	/// This can be called by <see cref="Workspace.AddWindow"/>, as an already focused window may
	/// have switched to a different workspace.
	/// </summary>
	/// <param name="window"></param>
	void OnWindowFocused(IWindow? window);

	/// <summary>
	/// Removes the given window from the <see cref="IWindowManager"/>, and fires
	/// <see cref="WindowRemovedTransform" />.
	/// </summary>
	/// <param name="window"></param>
	void OnWindowRemoved(IWindow window);
}
