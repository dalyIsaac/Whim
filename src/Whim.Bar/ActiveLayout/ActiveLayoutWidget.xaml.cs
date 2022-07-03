using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for ActiveLayoutWidget.xaml
/// </summary>
internal partial class ActiveLayoutWidget : UserControl
{
	/// <summary>
	/// The view model for the active layout widget.
	/// </summary>
	public ActiveLayoutWidgetViewModel ViewModel { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ActiveLayoutWidget"/> class.
	/// </summary>
	public ActiveLayoutWidget(IConfigContext config, IMonitor monitor)
	{
		ViewModel = new ActiveLayoutWidgetViewModel(config, monitor);
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "ActiveLayout/ActiveLayoutWidget");
	}

	/// <summary>
	/// Creates a new bar component which contains the active layout widget.
	/// </summary>
	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new ActiveLayoutWidget(configContext, monitor));
	}
}
