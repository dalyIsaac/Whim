using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for TextWidget.xaml
/// </summary>
public partial class TextWidget : UserControl
{
	public TextWidgetViewModel ViewModel { get; private set; }

	public TextWidget(string? value = null)
	{
		ViewModel = new TextWidgetViewModel(value);
		InitializeComponent();
		DataContext = ViewModel;
	}

	public static BarComponent CreateComponent(string? value = null)
	{
		return new BarComponent((configContext, monitor, window) => new TextWidget(value));
	}
}
