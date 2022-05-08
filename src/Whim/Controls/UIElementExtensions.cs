using Microsoft.UI.Xaml;
using Windows.Win32.Foundation;

namespace Whim;

public static class UIElementExtensions
{
	/// <summary>
	/// Initializes the <paramref name="component"/> with the specified namespace and path.
	/// This is necessary, as the build process will copy the plugins from
	/// the relevant project to the <c>/plugins</c> folder in <c>Whim.Runner</c>.
	/// As a result, the the <c>Uri</c> of <see cref="Application.LoadComponent"/>
	/// will be relative to the originating project, not the <c>Whim.Runner</c> project.
	/// </summary>
	/// <param name="component">The component to initialize.</param>
	/// <param name="componentNamespace">The namespace of the component.</param>
	/// <param name="componentPath">
	/// The path to the XAML component. Do not include the <c>.xaml</c> extension.
	/// </param>
	public static void InitializeComponent(object component, string componentNamespace, string componentPath)
	{
		System.Uri resourceLocator = new($"ms-appx:///plugins/{componentNamespace}/{componentPath}.xaml");
		Application.LoadComponent(component, resourceLocator, Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Nested);
	}

	public static IWindow InitializeBorderlessWindow(
		this Microsoft.UI.Xaml.Window uiWindow,
		string componentNamespace,
		string componentPath,
		IConfigContext configContext)
	{
		InitializeComponent(uiWindow, componentNamespace, componentPath);

		HWND hwnd = new(WinRT.Interop.WindowNative.GetWindowHandle(uiWindow));
		IWindow? window = Window.CreateWindow(GetHandle(uiWindow), configContext);
		if (window == null)
		{
			throw new InitializeBorderlessWindowException("Window was unexpectedly null");
		}

		Win32Helper.HideCaptionButtons(hwnd);
		Win32Helper.SetWindowCorners(hwnd);

		return window;
	}

	public static HWND GetHandle(this Microsoft.UI.Xaml.Window window)
	{
		return (HWND)WinRT.Interop.WindowNative.GetWindowHandle(window);
	}

	public static bool Hide(this Microsoft.UI.Xaml.Window window)
	{
		return Win32Helper.HideWindow(window.GetHandle());
	}
}
