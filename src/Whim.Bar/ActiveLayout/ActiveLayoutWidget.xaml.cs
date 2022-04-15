using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for ActiveLayoutWidget.xaml
/// </summary>
public partial class ActiveLayoutWidget : UserControl
{
	public ActiveLayoutWidgetViewModel ViewModel { get; private set; }

	public ActiveLayoutWidget(IConfigContext config, IMonitor monitor)
	{
		ViewModel = new ActiveLayoutWidgetViewModel(config, monitor);
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "ActiveLayout/ActiveLayoutWidget");
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new ActiveLayoutWidget(configContext, monitor));
	}
}
