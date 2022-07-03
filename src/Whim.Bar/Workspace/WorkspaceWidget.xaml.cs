using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for WorkspaceWidget.xaml
/// </summary>
public partial class WorkspaceWidget : UserControl
{
	/// <summary>
	/// The workspace view model.
	/// </summary>
	public WorkspaceWidgetViewModel ViewModel { get; }

	internal WorkspaceWidget(IConfigContext configContext, IMonitor monitor, Microsoft.UI.Xaml.Window window)
	{
		ViewModel = new WorkspaceWidgetViewModel(configContext, monitor);
		window.Closed += Window_Closed;
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "Workspace/WorkspaceWidget");
	}

	private void Window_Closed(object? sender, Microsoft.UI.Xaml.WindowEventArgs e)
	{
		ViewModel.Dispose();
	}

	/// <summary>
	/// Create the workspace widget bar component.
	/// </summary>
	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new WorkspaceWidget(configContext, monitor, window));
	}
}
