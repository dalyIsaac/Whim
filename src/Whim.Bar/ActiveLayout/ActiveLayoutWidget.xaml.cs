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
/// Interaction logic for ActiveLayoutWidget.xaml
/// </summary>
public partial class ActiveLayoutWidget : UserControl
{
	public ActiveLayoutWidgetViewModel ViewModel { get; private set; }

	public ActiveLayoutWidget(IConfigContext config, IMonitor monitor)
	{
		ViewModel = new ActiveLayoutWidgetViewModel(config, monitor);
		InitializeComponent();
		DataContext = ViewModel;
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new ActiveLayoutWidget(configContext, monitor));
	}
}
