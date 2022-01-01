using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for WorkspaceWidget.xaml
/// </summary>
public partial class WorkspaceWidget : UserControl
{
	public WorkspaceWidgetViewModel ViewModel { get; }

	public WorkspaceWidget(IConfigContext configContext, IMonitor monitor, System.Windows.Window window)
	{
		ViewModel = new WorkspaceWidgetViewModel(configContext, monitor);
		window.Closed += Window_Closed;
		InitializeComponent();
		DataContext = ViewModel;
	}

	private void Window_Closed(object? sender, EventArgs e)
	{
		ViewModel.Dispose();
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new WorkspaceWidget(configContext, monitor, window));
	}
}
