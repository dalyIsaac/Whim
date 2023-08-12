using Microsoft.UI.Xaml.Controls;

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

	internal LayoutPreviewWindowItem(IWindowState windowState)
	{
		WindowState = windowState;
		this.InitializeComponent();
	}
}
