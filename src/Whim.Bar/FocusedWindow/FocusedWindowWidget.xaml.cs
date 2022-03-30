using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for FocusedWindowWidget.xaml
/// </summary>
public partial class FocusedWindowWidget : UserControl
{
	public FocusedWindowWidgetViewModel ViewModel { get; private set; }

	public FocusedWindowWidget(IConfigContext configContext)
	{
		InitializeComponent();
		ViewModel = new FocusedWindowWidgetViewModel(configContext);
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new FocusedWindowWidget(configContext));
	}
}
