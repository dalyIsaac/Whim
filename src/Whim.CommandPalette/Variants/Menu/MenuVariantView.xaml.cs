using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

internal sealed partial class MenuVariantView : UserControl
{
	public MenuVariantViewModel ViewModel { get; }

	public MenuVariantView(MenuVariantViewModel viewModel)
	{
		ViewModel = viewModel;
		UIElementExtensions.InitializeComponent(this, "Whim.Component", "Variants/Menu");
	}

	private void ListViewItems_ItemClick(object sender, ItemClickEventArgs e)
	{
		Logger.Debug("Command palette item clicked");
		ListViewItems.SelectedItem = e.ClickedItem;
		ViewModel.ExecuteCommand();
	}

	public double GetViewHeight()
	{
		DependencyObject? container = ListViewItems.ContainerFromIndex(0);
		return (container is ListViewItem item) ? (item.ActualHeight * ListViewItems.Items.Count) : 0;
	}
}
