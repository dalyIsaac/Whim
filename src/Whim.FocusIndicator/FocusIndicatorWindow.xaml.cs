using Windows.Win32.Foundation;
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
	/// <param name="handle">The handle of the window to activate behind.</param>
	/// <param name="windowRectangle">The rectangle of the window to activate behind.</param>
	public void Activate(HWND handle, IRectangle<int> windowRectangle)
	{
		Logger.Verbose("Activating focus indicator window");
		int borderSize = FocusIndicatorConfig.BorderSize;

		IRectangle<int> borderRect = new Rectangle<int>()
		{
			X = windowRectangle.X - borderSize,
			Y = windowRectangle.Y - borderSize,
			Height = windowRectangle.Height + (borderSize * 2),
			Width = windowRectangle.Width + (borderSize * 2),
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
				handle,
				SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
			)
		);
	}

	public void Dispose()
	{
		_backdropController.Dispose();
	}
}
