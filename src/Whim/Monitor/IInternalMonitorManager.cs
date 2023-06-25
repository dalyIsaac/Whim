namespace Whim;

internal interface IInternalMonitorManager
{
	/// <summary>
	/// Called when the window has been focused.
	/// </summary>
	/// <param name="window"></param>
	internal void WindowFocused(IWindow window);
}
