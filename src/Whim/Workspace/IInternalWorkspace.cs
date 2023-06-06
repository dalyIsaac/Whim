namespace Whim;

/// <summary>
/// A workspace, with internal methods.
/// </summary>
internal interface IInternalWorkspace
{
	/// <summary>
	/// Called when a window is focused, regardless of whether it's in this workspace.
	/// </summary>
	/// <param name="window"></param>
	void WindowFocused(IWindow window);

	/// <summary>
	/// Called when a window is about to be minimized.
	/// </summary>
	/// <param name="window"></param>
	void WindowMinimizeStart(IWindow window);

	/// <summary>
	/// Called when a window is about to be unminimized.
	/// </summary>
	/// <param name="window"></param>
	void WindowMinimizeEnd(IWindow window);
}
