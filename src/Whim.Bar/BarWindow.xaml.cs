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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Win32.Foundation;

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

		WPFUI.Theme.Manager.SetSystemTheme(true);
		WPFUI.Theme.Watcher.Start();
		WPFUI.Background.Manager.Apply(this, true);
	}

	/// <summary>
	/// Renders the bar in the correct location. Use this instead of Show() to ensure
	/// the bar is rendered in the correct location.
	/// </summary>
	public void Render()
	{
		Left = _monitor.X + _barConfig.Margin;
		Top = _monitor.Y + _barConfig.Margin;
		Width = _monitor.Width - (_barConfig.Margin * 2);
		Height = _barConfig.Height;

		Show();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		Win32Helper.SetWindowCorners(new HWND(new WindowInteropHelper(this).Handle));
	}
}
