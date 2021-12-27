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
using Whim.Dashboard;
using Whim.Dashboard.Windows;
using Whim.Dashboard.Workspaces;

namespace Whim.Dashboard;

/// <summary>
/// Interaction logic for DashboardWindow.xaml
/// </summary>
public partial class DashboardWindow : System.Windows.Window, IDisposable
{
	private readonly IConfigContext _configContext;

	private readonly WorkspaceDashboard? _workspaceDashboard;
	private readonly WindowsDashboard? _registeredWindows;

	private bool disposedValue;

	public DashboardWindow(IConfigContext configContext)
	{
		_configContext = configContext;

		InitializeComponent();

		_workspaceDashboard = new(configContext);
		Grid.Children.Add(_workspaceDashboard);
		Grid.SetRow(_workspaceDashboard, 0);

		_registeredWindows = new(configContext);
		Grid.Children.Add(_registeredWindows);
		Grid.SetRow(_registeredWindows, 1);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_workspaceDashboard?.Dispose();
				_registeredWindows?.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		Logger.Debug("Closing Dashboard window");
		e.Cancel = true;
		Hide();
	}
}
