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
/// Interaction logic for TextWidget.xaml
/// </summary>
public partial class TextWidget : UserControl
{
	public TextWidgetViewModel ViewModel { get; private set; }

	public TextWidget(string? value = null)
	{
		ViewModel = new TextWidgetViewModel(value);
		InitializeComponent();
		DataContext = ViewModel;
	}

	public static BarComponent CreateComponent(string? value = null)
	{
		return new BarComponent((configContext) => new TextWidget(value));
	}
}
