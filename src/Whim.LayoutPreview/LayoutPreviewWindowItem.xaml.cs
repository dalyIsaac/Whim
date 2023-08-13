using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Whim.LayoutPreview;

/// <summary>
/// Interaction logic for LayoutPreviewWindowItem.xaml
/// </summary>
public sealed partial class LayoutPreviewWindowItem : UserControl
{
	/// <summary>
	/// The <see cref="IWindowState"/> that this <see cref="LayoutPreviewWindowItem"/> represents.
	/// </summary>
	public IWindowState WindowState { get; }

	public ImageSource ImageSource { get; }

	internal LayoutPreviewWindowItem(IWindowState windowState)
	{
		WindowState = windowState;
		ImageSource = IconHelper.GetIcon(windowState.Window);
		InitializeComponent();
	}
}
