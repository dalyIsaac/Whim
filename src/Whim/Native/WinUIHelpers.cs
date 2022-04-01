using Windows.Win32.Foundation;

namespace Whim;

public static class WinUIHelpers
{
	public static HWND GetHandle(this Microsoft.UI.Xaml.Window window)
	{
		return (HWND)WinRT.Interop.WindowNative.GetWindowHandle(window);
	}

	public static bool Hide(this Microsoft.UI.Xaml.Window window)
	{
		return Win32Helper.HideWindow(window.GetHandle());
	}
}
