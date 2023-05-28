using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
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
	/// <param name="context"></param>
	/// <returns></returns>
	public static bool Hide(this Microsoft.UI.Xaml.Window window, IContext context)
	{
		return context.NativeManager.HideWindow(window.GetHandle());
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
	/// <param name="context"></param>
	/// <param name="componentNamespace"></param>
	/// <param name="componentPath"></param>
	/// <returns></returns>
	/// <exception cref="InitializeWindowException">
	/// When an <see cref="IWindow"/> cannot be created from the handle of the given
	/// <paramref name="uiWindow"/>.
	/// </exception>
	public static IWindow InitializeBorderlessWindow(
		this Microsoft.UI.Xaml.Window uiWindow,
		IContext context,
		string componentNamespace,
		string componentPath
	)
	{
		UIElementExtensions.InitializeComponent(uiWindow, componentNamespace, componentPath);

		HWND hwnd = new(WinRT.Interop.WindowNative.GetWindowHandle(uiWindow));
		IWindow window =
			context.WindowManager.CreateWindow(GetHandle(uiWindow))
			?? throw new InitializeWindowException("Window was unexpectedly null");

		context.NativeManager.HideCaptionButtons(hwnd);
		context.NativeManager.SetWindowCorners(hwnd);

		return window;
	}

	/// <summary>
	/// Sets the given <paramref name="window"/>'s <see cref="Microsoft.UI.Xaml.Window.SystemBackdrop"/>
	/// to a <see cref="MicaBackdrop"/> if the current system supports it, otherwise a
	/// <see cref="DesktopAcrylicBackdrop"/>.
	/// </summary>
	/// <param name="window"></param>
	public static void SetSystemBackdrop(this Microsoft.UI.Xaml.Window window)
	{
		window.SystemBackdrop = MicaController.IsSupported()
			? new MicaBackdrop() { Kind = MicaKind.BaseAlt }
			: new DesktopAcrylicBackdrop() { };
	}
}
