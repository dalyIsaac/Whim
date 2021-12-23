using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Whim.Dashboard.Controls.ViewModel;

namespace Whim.Dashboard.Controls;

/// <summary>
/// Interaction logic for RegisteredWindows.xaml
/// </summary>
public partial class RegisteredWindows : UserControl
{
	private readonly RegisteredWindowsViewModel _viewModel;

	public RegisteredWindows(IConfigContext configContext)
	{
		_viewModel = new RegisteredWindowsViewModel(configContext);

		InitializeComponent();
		DataContext = _viewModel;
	}

	/// <summary>
	/// Enables sorting for grid columns.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
	{
		GridViewColumnHeader? column = sender as GridViewColumnHeader;
		string? sortBy = column?.Tag.ToString();

		if (sortBy == null)
		{
			return;
		}

		SortDescriptionCollection sdc = RegisteredWindowsListView.Items.SortDescriptions;
		ListSortDirection sortDirection = ListSortDirection.Ascending;

		if (sdc.Count > 0)
		{
			SortDescription sd = sdc[0];
			sortDirection = (ListSortDirection)(((int)sd.Direction + 1) % 2);
			sdc.Clear();
		}
		sdc.Add(new SortDescription(sortBy, sortDirection));
	}
}
