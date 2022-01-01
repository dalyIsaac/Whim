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
/// Interaction logic for DateTimeWidget.xaml
/// </summary>
public partial class DateTimeWidget : UserControl
{
	public DateTimeWidgetViewModel ViewModel { get; private set; }

	public DateTimeWidget(int interval = 100, string format = "HH:mm:ss dd-MMM-yyyy")
	{
		ViewModel = new DateTimeWidgetViewModel(interval, format);
		InitializeComponent();
		DataContext = ViewModel;
	}


	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new DateTimeWidget());
	}
}
