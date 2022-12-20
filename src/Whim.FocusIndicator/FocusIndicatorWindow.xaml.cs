using Windows.Win32.Foundation;

namespace Whim.FocusIndicator;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class FocusIndicatorWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	public FocusIndicatorConfig FocusIndicatorConfig { get; }
	private readonly IWindow _window;

	public FocusIndicatorWindow(IConfigContext configContext, FocusIndicatorConfig focusIndicatorConfig)
	{
		_configContext = configContext;
		FocusIndicatorConfig = focusIndicatorConfig;
		_window = this.InitializeBorderlessWindow(configContext, "Whim.FocusIndicator", "FocusIndicatorWindow");

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

		ILocation<int> borderLocation = new Location<int>()
		{
			X = focusedWindowLocation.X - borderSize,
			Y = focusedWindowLocation.Y - borderSize,
			Height = focusedWindowLocation.Height + (borderSize * 2),
			Width = focusedWindowLocation.Width + (borderSize * 2)
		};

		this.SetIsShownInSwitchers(false);

		// Prevent the window from being activated.
		_configContext.NativeManager.PreventWindowActivation(_window.Handle);

		WindowDeferPosHandle.SetWindowPosFixScaling(
			_configContext,
			windowState: new WindowState()
			{
				Window = _window,
				Location = borderLocation,
				WindowSize = WindowSize.Normal
			},
			monitor: _configContext.MonitorManager.FocusedMonitor,
			hwndInsertAfter: new HWND(1)
		);
	}
}
