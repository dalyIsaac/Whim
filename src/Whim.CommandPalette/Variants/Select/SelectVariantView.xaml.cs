using Microsoft.UI.Xaml.Controls;
using System;

namespace Whim.CommandPalette;

internal sealed partial class SelectVariantView : UserControl
{
	public SelectVariantViewModel ViewModel { get; }

	public static double NonIdealStateHeight => 36;

	public SelectVariantView(SelectVariantViewModel viewModel)
	{
		ViewModel = viewModel;
		viewModel.ScrollIntoViewRequested += ViewModel_ScrollIntoViewRequested;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/Select/SelectVariantView");
	}

	private void ViewModel_ScrollIntoViewRequested(object? sender, EventArgs e)
	{
		ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
	}

	private void ListViewItems_ItemClick(object sender, ItemClickEventArgs e)
	{
		Logger.Debug("Command palette item clicked");
		int idx = ViewModel.SelectRows.IndexOf((IVariantRowView<SelectOption, SelectVariantRowViewModel>)e.ClickedItem);

		if (idx >= 0)
		{
			ViewModel.SelectedIndex = idx;
			ViewModel.UpdateSelectedItem();
		}
	}

	public double GetViewMaxHeight()
	{
		if (ViewModel.SelectRows.Count == 0)
		{
			return NonIdealStateHeight;
		}

		// On first render, there'll be no items, so return maximum possible height.
		if (ListViewItems.ContainerFromIndex(0) is not ListViewItem listViewItem)
		{
			return double.PositiveInfinity;
		}

		return listViewItem.ActualHeight * ListViewItems.Items.Count;
	}
}
