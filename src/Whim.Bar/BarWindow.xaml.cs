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
using System.Windows.Shapes;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class BarWindow : System.Windows.Window
{
	private readonly IConfigContext _configContext;
	private readonly BarConfig _barConfig;
	private readonly IMonitor _monitor;

	public BarWindow(IConfigContext configContext, BarConfig barConfig, IMonitor monitor)
	{
		_configContext = configContext;
		_barConfig = barConfig;
		_monitor = monitor;

		InitializeComponent();

		// Set up the bar
		LeftPanel.Children.AddRange(_barConfig.LeftComponents.Select(c => c(_configContext, _monitor, this)));
		CenterPanel.Children.AddRange(_barConfig.CenterComponents.Select(c => c(_configContext, _monitor, this)));
		RightPanel.Children.AddRange(_barConfig.RightComponents.Select(c => c(_configContext, _monitor, this)));
	}

	/// <summary>
	/// Renders the bar in the correct location. Use this instead of Show() to ensure
	/// the bar is rendered in the correct location.
	/// </summary>
	public void Render()
	{
		Left = _monitor.X;
		Top = _monitor.Y;
		Width = _monitor.Width;
		Height = _barConfig.Height;

		Show();
	}
}
