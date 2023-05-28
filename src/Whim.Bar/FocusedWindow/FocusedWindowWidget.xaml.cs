using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for FocusedWindowWidget.xaml
/// </summary>
public partial class FocusedWindowWidget : UserControl
{
	/// <summary>
	/// The focused window view model.
	/// </summary>
	public FocusedWindowWidgetViewModel ViewModel { get; private set; }

	internal FocusedWindowWidget(IContext context, IMonitor monitor)
	{
		ViewModel = new FocusedWindowWidgetViewModel(context, monitor);
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "FocusedWindow/FocusedWindowWidget");
	}

	/// <summary>
	/// Create the focused window widget bar component.
	/// </summary>
	public static BarComponent CreateComponent()
	{
		return new BarComponent((context, monitor, window) => new FocusedWindowWidget(context, monitor));
	}
}
