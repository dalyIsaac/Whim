using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for ActiveLayoutWidget.xaml
/// </summary>
public partial class BatteryWidget : UserControl
{
	internal BatteryWidgetViewModel ViewModel { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ActiveLayoutWidget"/> class.
	/// </summary>
	internal BatteryWidget(IContext context, IMonitor monitor)
	{
		ViewModel = new BatteryWidgetViewModel(context);
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "Battery/BatteryWidget");
	}

	/// <summary>
	/// Creates a new bar component which contains the active layout widget.
	/// </summary>
	public static BarComponent CreateComponent() => new BatteryComponent();
}

/// <summary>
/// The bar component for the active layout widget.
/// </summary>
public record BatteryComponent : BarComponent
{
	/// <inheritdoc/>
	public override UserControl CreateWidget(IContext context, IMonitor monitor, Microsoft.UI.Xaml.Window window) =>
		new BatteryWidget(context, monitor);
}
