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
	void WindowFocused(IWindow? window);
}
