using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

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
		//_window = this.InitializeBorderlessWindow(_context, "Whim.LayoutPreview", "LayoutPreviewWindow");
		UIElementExtensions.InitializeComponent(this, "Whim.LayoutPreview", "LayoutPreviewWindow");
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
	}
}
