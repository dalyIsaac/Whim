using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.FocusIndicator;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class FocusIndicatorWindow : Microsoft.UI.Xaml.Window, System.IDisposable
{
	private readonly IContext _context;
	public FocusIndicatorConfig FocusIndicatorConfig { get; }
	private readonly WindowBackdropController _backdropController;
	private readonly IWindow _window;

	public FocusIndicatorWindow(IContext context, FocusIndicatorConfig focusIndicatorConfig)
	{
		_context = context;
		FocusIndicatorConfig = focusIndicatorConfig;
		_window = this.InitializeBorderlessWindow(context, "Whim.FocusIndicator", "FocusIndicatorWindow");

		this.SetIsShownInSwitchers(false);
		_backdropController = new(this, focusIndicatorConfig.Backdrop);

		Title = FocusIndicatorConfig.Title;
	}

	/// <summary>
	/// Activates the window behind the given window.
	/// </summary>
	/// <param name="targetWindowState">The window to show the indicator for.</param>
	public void Activate(IWindowState targetWindowState)
	{
		Logger.Debug("Activating focus indicator window");
		IRectangle<int> focusedWindowRect = targetWindowState.Rectangle;
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
			new DeferWindowPosState(
				_window.Handle,
				WindowSize.Normal,
				borderRect,
				targetWindowState.Window.Handle,
				SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
			)
		);
	}

	public void Dispose()
	{
		_backdropController.Dispose();
	}
}
