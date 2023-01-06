using Microsoft.UI.Xaml.Controls;
using System;

namespace Whim.CommandPalette;

internal sealed partial class SelectVariantView : UserControl
{
	public SelectVariantViewModel ViewModel { get; }

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
		ListViewItems.SelectedItem = e.ClickedItem;
		ViewModel.ToggleSelectedItem();
	}

	public double GetViewMaxHeight()
	{
		return ViewModel.SelectRows.Count * SelectRow.SelectRowHeight;
	}
}
