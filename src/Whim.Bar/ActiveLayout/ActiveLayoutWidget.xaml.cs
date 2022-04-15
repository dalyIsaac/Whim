namespace Whim.Bar;

/// <summary>
/// Interaction logic for ActiveLayoutWidget.xaml
/// </summary>
public partial class ActiveLayoutWidget : PluginControl
{
	public ActiveLayoutWidgetViewModel ViewModel { get; private set; }

	public ActiveLayoutWidget(IConfigContext config, IMonitor monitor)
	{
		ViewModel = new ActiveLayoutWidgetViewModel(config, monitor);
		InitializeComponent("Whim.Bar", "ActiveLayout/ActiveLayoutWidget");
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new ActiveLayoutWidget(configContext, monitor));
	}
}
