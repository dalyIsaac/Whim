using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

internal sealed partial class FreeTextVariantView : UserControl
{
	public FreeTextVariantViewModel ViewModel { get; }

	public FreeTextVariantView(FreeTextVariantViewModel viewModel)
	{
		ViewModel = viewModel;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/FreeText/FreeTextVariantView");
	}

	public double GetViewHeight() => Prompt.ActualHeight;
}
