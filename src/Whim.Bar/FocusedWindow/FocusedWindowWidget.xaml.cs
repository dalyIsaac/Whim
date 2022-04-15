namespace Whim.Bar;

/// <summary>
/// Interaction logic for FocusedWindowWidget.xaml
/// </summary>
public partial class FocusedWindowWidget : PluginControl
{
	public FocusedWindowWidgetViewModel ViewModel { get; private set; }

	public FocusedWindowWidget(IConfigContext configContext)
	{
		ViewModel = new FocusedWindowWidgetViewModel(configContext);
		InitializeComponent("Whim.Bar", "FocusedWindow/FocusedWindowWidget");
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new FocusedWindowWidget(configContext));
	}
}
