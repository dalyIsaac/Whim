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
		IRectangle<int> focusedWindowRect = windowState.Rectangle;
		int borderSize = FocusIndicatorConfig.BorderSize;

		IRectangle<int> borderRect = new Rectangle<int>()
		{
			X = focusedWindowRect.X - borderSize,
			Y = focusedWindowRect.Y - borderSize,
			Height = focusedWindowRect.Height + (borderSize * 2),
			Width = focusedWindowRect.Width + (borderSize * 2)
		};

		// Prevent the window from being activated.
		_context.NativeManager.PreventWindowActivation(_window.Handle);

		using DeferWindowPosHandle windowDeferPos = _context.NativeManager.DeferWindowPos();

		// Layout the focus indicator window.
		windowDeferPos.DeferWindowPos(
			new WindowState()
			{
				Window = _window,
				Rectangle = borderRect,
				WindowSize = WindowSize.Normal
			},
			windowState.Window.Handle,
			SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		);
	}
}
