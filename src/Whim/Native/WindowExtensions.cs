using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Win32.Foundation;

namespace Whim;

public static class WindowExtensions
{
	/// <summary>
	/// Gets the handle of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static HWND GetHandle(this Microsoft.UI.Xaml.Window window)
	{
		return (HWND)WinRT.Interop.WindowNative.GetWindowHandle(window);
	}

	/// <summary>
	/// Hides the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static bool Hide(this Microsoft.UI.Xaml.Window window)
	{
		return Win32Helper.HideWindow(window.GetHandle());
	}

	/// <summary>
	/// Gets the associated <see cref="AppWindow"/> instance of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static AppWindow GetAppWindow(this Microsoft.UI.Xaml.Window window)
	{
		WindowId windowId = Win32Interop.GetWindowIdFromWindow(window.GetHandle());
		return AppWindow.GetFromWindowId(windowId);
	}

	/// <summary>
	/// Sets a value indicating whether the given <paramref name="window"/> will appear in various
	/// system representations, such as ALT+TAB and taskbar.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="show"></param>
	public static void SetIsShownInSwitchers(this Microsoft.UI.Xaml.Window window, bool show)
	{
		window.GetAppWindow().IsShownInSwitchers = show;
	}
}
