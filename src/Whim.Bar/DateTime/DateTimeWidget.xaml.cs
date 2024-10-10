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
	internal DateTimeWidgetViewModel ViewModel { get; private set; }

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
	public static BarComponent CreateComponent(int interval = 100, string format = "HH:mm:ss dd-MMM-yyyy") =>
		new DateTimeComponent(interval, format);
}

/// <summary>
/// The bar component for the date time widget.
/// </summary>
/// <param name="IntervalMs">
/// The interval in milliseconds to update the date time.
/// </param>
/// <param name="Format">
/// The date time format. See <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings">Custom date and time format strings</see>.
/// </param>
public record DateTimeComponent(int IntervalMs, string Format) : BarComponent
{
	/// <inheritdoc/>
	public override UserControl CreateWidget(IContext context, IMonitor monitor, Microsoft.UI.Xaml.Window window) =>
		new DateTimeWidget(IntervalMs, Format);
}
