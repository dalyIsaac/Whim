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
using Whim.Controls.ViewModel;
using Whim.Core;

namespace Whim.Controls;

/// <summary>
/// Interaction logic for WorkspaceDashboard.xaml
/// </summary>
public partial class WorkspaceDashboard : UserControl
{
	private readonly WorkspaceDashboardViewModel _viewModel;

	public WorkspaceDashboard(IConfigContext configContext)
	{
		_viewModel = new WorkspaceDashboardViewModel(configContext);

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

		SortDescriptionCollection sdc = WorkspaceDashboardListView.Items.SortDescriptions;
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
