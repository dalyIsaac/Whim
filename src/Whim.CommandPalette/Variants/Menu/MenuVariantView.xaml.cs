using Microsoft.UI.Xaml.Controls;
using System;

namespace Whim.CommandPalette;

internal sealed partial class MenuVariantView : UserControl
{
	public MenuVariantViewModel ViewModel { get; }

	/// <summary>
	/// The height of a row, including the surrounding padding/margin.
	/// </summary>
	public static double RowHeight => MenuVariantRowView.MenuRowHeight + (2 * 2);

	public MenuVariantView(MenuVariantViewModel viewModel)
	{
		ViewModel = viewModel;
		viewModel.ScrollIntoViewRequested += ViewModel_ScrollIntoViewRequested;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/Menu/MenuVariantView");
	}

	private void ViewModel_ScrollIntoViewRequested(object? sender, EventArgs e)
	{
		ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
	}

	private void ListViewItems_ItemClick(object sender, ItemClickEventArgs e)
	{
		Logger.Debug("Command palette item clicked");
		ListViewItems.SelectedItem = e.ClickedItem;
		ViewModel.ExecuteCommand();
	}

	public double GetViewMaxHeight() =>
		ViewModel.MenuRows.Count == 0 ? RowHeight : RowHeight * ViewModel.MenuRows.Count;
}
