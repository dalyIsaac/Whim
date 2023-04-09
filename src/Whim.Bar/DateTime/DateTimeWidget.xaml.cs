using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for DateTimeWidget.xaml
/// </summary>
public partial class DateTimeWidget : UserControl
{
	/// <summary>
	/// The date time view model.
	/// </summary>
	public DateTimeWidgetViewModel ViewModel { get; private set; }

	/// <summary>
	/// Create the date time widget.
	/// </summary>
	/// <param name="interval"></param>
	/// <param name="format"></param>
	internal DateTimeWidget(int interval, string format)
	{
		ViewModel = new DateTimeWidgetViewModel(interval, format);
		DataContext = ViewModel;
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "DateTime/DateTimeWidget");
	}

	/// <summary>
	/// Create the date time widget bar component.
	/// </summary>
	/// <param name="interval"></param>
	/// <param name="format"></param>
	/// <returns></returns>
	public static BarComponent CreateComponent(int interval = 100, string format = "HH:mm:ss dd-MMM-yyyy")
	{
		return new BarComponent((context, monitor, window) => new DateTimeWidget(interval, format));
	}
}
