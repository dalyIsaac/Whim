namespace Whim.Bar;

/// <summary>
/// Interaction logic for TextWidget.xaml
/// </summary>
public partial class TextWidget : PluginControl
{
	public TextWidgetViewModel ViewModel { get; private set; }

	public TextWidget(string? value = null)
	{
		ViewModel = new TextWidgetViewModel(value);
		InitializeComponent("Whim.Bar", "Text/TextWidget");
	}

	public static BarComponent CreateComponent(string? value = null)
	{
		return new BarComponent((configContext, monitor, window) => new TextWidget(value));
	}
}
