using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.LayoutPreview;

/// <summary>
/// Window showing a preview of the layout.
/// </summary>
public sealed partial class LayoutPreviewWindow : Window
{
	private readonly IContext _context;
	private readonly IWindow _window;

	/// <summary>
	/// Initializes a new instance of the <see cref="LayoutPreviewWindow"/> class.
	/// </summary>
	public LayoutPreviewWindow(IContext context)
	{
		_context = context;
		_window = this.InitializeBorderlessWindow(_context, "Whim.LayoutPreview", "LayoutPreviewWindow");
		this.SetIsShownInSwitchers(false);
		this.SetSystemBackdrop();

		Title = LayoutPreviewConfig.Title;
	}

	public void Activate(IWindow movingWindow, IMonitor? monitor)
	{
		if (monitor == null)
		{
			return;
		}

		using WindowDeferPosHandle handle = new(_context);
		handle.DeferWindowPos(
			new WindowState()
			{
				Window = _window,
				Location = monitor.WorkingArea,
				WindowSize = WindowSize.Normal
			},
			movingWindow.Handle,
			SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
		);
	}

	public void Update(IWindowState[] windowStates)
	{
		LayoutPreviewWindowItem[] items = new LayoutPreviewWindowItem[windowStates.Length];
		for (int i = 0; i < windowStates.Length; i++)
		{
			items[i] = new LayoutPreviewWindowItem(windowStates[i]);

			Canvas.SetLeft(items[i], windowStates[i].Location.X);
			Canvas.SetTop(items[i], windowStates[i].Location.Y);
		}

		LayoutPreviewCanvas.Children.Clear();
		LayoutPreviewCanvas.Children.AddRange(items);
		LayoutPreviewCanvas.InvalidateMeasure();
	}
}
