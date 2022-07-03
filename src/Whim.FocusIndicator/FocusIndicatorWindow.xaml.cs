namespace Whim.FocusIndicator;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FocusIndicatorWindow : Microsoft.UI.Xaml.Window
{
	public FocusIndicatorConfig FocusIndicatorConfig { get; }
	private readonly IWindow _window;

	public FocusIndicatorWindow(IConfigContext configContext, FocusIndicatorConfig focusIndicatorConfig)
	{
		FocusIndicatorConfig = focusIndicatorConfig;
		_window = this.InitializeBorderlessWindow("Whim.FocusIndicator", "FocusIndicatorWindow", configContext);

		Title = FocusIndicatorConfig.Title;
	}

	/// <summary>
	/// Activates the window behind the given window.
	/// </summary>
	/// <param name="windowLocation">The location of the window.</param>
	public void Activate(IWindowState windowLocation)
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
			new WindowState(_window, borderLocation, WindowSize.Normal),
			windowLocation.Window.Handle
		);
	}
}
