namespace Whim.Bar;

/// <summary>
/// Interaction logic for WorkspaceWidget.xaml
/// </summary>
public partial class WorkspaceWidget : PluginControl
{
	public WorkspaceWidgetViewModel ViewModel { get; }

	public WorkspaceWidget(IConfigContext configContext, IMonitor monitor, Microsoft.UI.Xaml.Window window)
	{
		ViewModel = new WorkspaceWidgetViewModel(configContext, monitor);
		window.Closed += Window_Closed;
		InitializeComponent("Whim.Bar", "Workspace/WorkspaceWidget");
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
