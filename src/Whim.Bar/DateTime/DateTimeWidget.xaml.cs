namespace Whim.Bar;

/// <summary>
/// Interaction logic for DateTimeWidget.xaml
/// </summary>
public partial class DateTimeWidget : PluginControl
{
	public DateTimeWidgetViewModel ViewModel { get; private set; }

	public DateTimeWidget(int interval = 100, string format = "HH:mm:ss dd-MMM-yyyy")
	{
		ViewModel = new DateTimeWidgetViewModel(interval, format);
		DataContext = ViewModel;
		InitializeComponent("Whim.Bar", "DateTime/DateTimeWidget");
	}


	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new DateTimeWidget());
	}
}
