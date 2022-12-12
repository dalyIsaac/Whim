using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

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

		ILocation<int> borderLocation = new Location<int>()
		{
			X = focusedWindowLocation.X - borderSize,
			Y = focusedWindowLocation.Y - borderSize,
			Height = focusedWindowLocation.Height + (borderSize * 2),
			Width = focusedWindowLocation.Width + (borderSize * 2)
		};

		this.SetIsShownInSwitchers(false);

		// Prevent the window from being activated.
		_ = PInvoke.SetWindowLong(
			this.GetHandle(),
			WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
			(int)WINDOW_EX_STYLE.WS_EX_NOACTIVATE
		);

		WindowDeferPosHandle.SetWindowPosFixScaling(
			_configContext,
			windowState: new WindowState()
			{
				Window = _window,
				Location = borderLocation,
				WindowSize = WindowSize.Normal
			},
			monitorManager: _configContext.MonitorManager,
			monitor: _configContext.MonitorManager.FocusedMonitor,
			hwndInsertAfter: new HWND(1)
		);
	}
}
