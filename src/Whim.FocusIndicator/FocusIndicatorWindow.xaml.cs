using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.FocusIndicator;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class FocusIndicatorWindow : Microsoft.UI.Xaml.Window
{
	private readonly IContext _context;
	public FocusIndicatorConfig FocusIndicatorConfig { get; }
	private readonly IWindow _window;

	public FocusIndicatorWindow(IContext context, FocusIndicatorConfig focusIndicatorConfig)
	{
		_context = context;
		FocusIndicatorConfig = focusIndicatorConfig;
		_window = this.InitializeBorderlessWindow(context, "Whim.FocusIndicator", "FocusIndicatorWindow");

		this.SetIsShownInSwitchers(false);
		this.SetSystemBackdrop();

		Title = FocusIndicatorConfig.Title;
	}

	/// <summary>
	/// Activates the window behind the given window.
	/// </summary>
	/// <param name="windowState">The window to show the indicator for.</param>
	public void Activate(IWindowState windowState)
	{
		Logger.Debug("Activating focus indicator window");
		ILocation<int> focusedWindowLocation = windowState.Location;
		int borderSize = FocusIndicatorConfig.BorderSize;

		ILocation<int> borderLocation = new Location<int>()
		{
			X = focusedWindowLocation.X - borderSize,
			Y = focusedWindowLocation.Y - borderSize,
			Height = focusedWindowLocation.Height + (borderSize * 2),
			Width = focusedWindowLocation.Width + (borderSize * 2)
		};

		// Prevent the window from being activated.
		_context.NativeManager.PreventWindowActivation(_window.Handle);

		using DeferWindowPosHandle windowDeferPos = _context.NativeManager.DeferWindowPos();

		// Layout the focus indicator window.
		windowDeferPos.DeferWindowPos(
			new WindowState()
			{
				Window = _window,
				Location = borderLocation,
				WindowSize = WindowSize.Normal
			},
			windowState.Window.Handle,
			SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		);
	}
}
