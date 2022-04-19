using System;
using Windows.Win32.Foundation;

namespace Whim.FocusIndicator;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FocusIndicatorWindow : Microsoft.UI.Xaml.Window
{
	public  FocusIndicatorConfig FocusIndicatorConfig { get; }
	private readonly IWindow _window;

	public FocusIndicatorWindow(IConfigContext configContext, FocusIndicatorConfig focusIndicatorConfig)
	{
		FocusIndicatorConfig = focusIndicatorConfig;
		UIElementExtensions.InitializeComponent(this, "Whim.FocusIndicator", "FocusIndicatorWindow");

		Title = FocusIndicatorConfig.Title;

		HWND hwnd = new(WinRT.Interop.WindowNative.GetWindowHandle(this));
		IWindow? window = Window.CreateWindow(this.GetHandle(), configContext);
		if (window == null)
		{
			throw new FocusIndicatorException("Window was unexpectedly null");
		}
		_window = window;

		Win32Helper.HideCaptionButtons(hwnd);
		Win32Helper.SetWindowCorners(hwnd);
	}

	/// <summary>
	/// Activates the window behind the given window.
	/// </summary>
	/// <param name="windowLocation">The location of the window.</param>
	public void Activate(IWindowLocation windowLocation)
	{
		ILocation<int> focusedWindowLocation = windowLocation.Location;
		int borderSize = FocusIndicatorConfig.BorderSize;

		ILocation<int> borderLocation = new Location(
			x: focusedWindowLocation.X - borderSize,
			y: focusedWindowLocation.Y - borderSize,
			height: focusedWindowLocation.Height + (borderSize * 2),
			width: focusedWindowLocation.Width + (borderSize * 2)
		);

		Win32Helper.SetWindowPos(
			new WindowLocation(_window, borderLocation, WindowState.Normal),
			windowLocation.Window.Handle
		);
	}
}
