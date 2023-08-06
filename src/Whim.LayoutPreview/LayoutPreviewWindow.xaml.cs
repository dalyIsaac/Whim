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
		Rectangle[] rectangles = new Rectangle[windowStates.Length];
		for (int i = 0; i < windowStates.Length; i++)
		{
			rectangles[i] = new Rectangle()
			{
				Fill = new SolidColorBrush(Colors.Red),
				Width = windowStates[i].Location.Width,
				Height = windowStates[i].Location.Height
			};

			Canvas.SetLeft(rectangles[i], windowStates[i].Location.X);
			Canvas.SetTop(rectangles[i], windowStates[i].Location.Y);
		}

		LayoutPreviewCanvas.Children.Clear();
		LayoutPreviewCanvas.Children.AddRange(rectangles);
	}
}
