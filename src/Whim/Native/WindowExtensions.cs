using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Extensions for <see cref="Microsoft.UI.Xaml.Window"/>.
/// </summary>
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
	/// <param name="configContext"></param>
	/// <returns></returns>
	public static bool Hide(this Microsoft.UI.Xaml.Window window, IConfigContext configContext)
	{
		return configContext.NativeManager.HideWindow(window.GetHandle());
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

	/// <summary>
	/// Initializes the given <paramref name="uiWindow"/> as a borderless window.
	/// </summary>
	/// <param name="uiWindow"></param>
	/// <param name="configContext"></param>
	/// <param name="componentNamespace"></param>
	/// <param name="componentPath"></param>
	/// <returns></returns>
	/// <exception cref="InitializeWindowException">
	/// When an <see cref="IWindow"/> cannot be created from the handle of the given
	/// <paramref name="uiWindow"/>.
	/// </exception>
	public static IWindow InitializeBorderlessWindow(
		this Microsoft.UI.Xaml.Window uiWindow,
		IConfigContext configContext,
		string componentNamespace,
		string componentPath
	)
	{
		UIElementExtensions.InitializeComponent(uiWindow, componentNamespace, componentPath);

		HWND hwnd = new(WinRT.Interop.WindowNative.GetWindowHandle(uiWindow));
		IWindow? window = configContext.WindowManager.CreateWindow( GetHandle(uiWindow));
		if (window == null)
		{
			throw new InitializeWindowException("Window was unexpectedly null");
		}

		configContext.NativeManager.HideCaptionButtons(hwnd);
		configContext.NativeManager.SetWindowCorners(hwnd);

		return window;
	}
}
