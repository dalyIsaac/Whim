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
using Whim.Dashboard.Controls;
using Whim.Core;

namespace Whim.Dashboard;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : System.Windows.Window
{
	private readonly IConfigContext _configContext;

	public MainWindow(IConfigContext configContext)
	{
		_configContext = configContext;

		InitializeComponent();

		WorkspaceDashboard dashboard = new(configContext);
		Grid.Children.Add(dashboard);
		Grid.SetRow(dashboard, 0);

		RegisteredWindows windows = new(configContext);
		Grid.Children.Add(windows);
		Grid.SetRow(windows, 1);
	}
}
