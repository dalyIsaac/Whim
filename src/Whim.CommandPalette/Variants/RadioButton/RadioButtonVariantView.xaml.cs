using Microsoft.UI.Xaml.Controls;
using System;

namespace Whim.CommandPalette;

internal sealed partial class RadioButtonVariantView : UserControl
{
	public SelectVariantViewModel ViewModel { get; }

	public RadioButtonVariantView(SelectVariantViewModel viewModel)
	{
		ViewModel = viewModel;
		viewModel.ScrollIntoViewRequested += ViewModel_ScrollIntoViewRequested;
		UIElementExtensions.InitializeComponent(
			this,
			"Whim.CommandPalette",
			"Variants/RadioButton/RadioButtonVariantView"
		);
	}

	private void ViewModel_ScrollIntoViewRequested(object? sender, EventArgs e)
	{
		ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
	}
}
