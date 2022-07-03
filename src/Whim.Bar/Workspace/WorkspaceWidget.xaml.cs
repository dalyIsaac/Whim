using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for WorkspaceWidget.xaml
/// </summary>
internal partial class WorkspaceWidget : UserControl
{
	public WorkspaceWidgetViewModel ViewModel { get; }

	public WorkspaceWidget(IConfigContext configContext, IMonitor monitor, Microsoft.UI.Xaml.Window window)
	{
		ViewModel = new WorkspaceWidgetViewModel(configContext, monitor);
		window.Closed += Window_Closed;
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "Workspace/WorkspaceWidget");
	}

	private void Window_Closed(object? sender, Microsoft.UI.Xaml.WindowEventArgs e)
	{
		ViewModel.Dispose();
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new WorkspaceWidget(configContext, monitor, window));
	}
}
