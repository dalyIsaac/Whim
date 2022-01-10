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
/// Interaction logic for FocusedWindowWidget.xaml
/// </summary>
public partial class FocusedWindowWidget : UserControl
{
	public FocusedWindowWidgetViewModel ViewModel { get; private set; }

	public FocusedWindowWidget(IConfigContext configContext)
	{
		InitializeComponent();
		ViewModel = new FocusedWindowWidgetViewModel(configContext);
		DataContext = ViewModel;
	}

	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new FocusedWindowWidget(configContext));
	}
}
