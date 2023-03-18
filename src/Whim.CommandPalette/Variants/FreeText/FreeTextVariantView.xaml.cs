using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

internal sealed partial class FreeTextVariantView : UserControl
{
	public static double ViewHeight => 40;

	public FreeTextVariantViewModel ViewModel { get; }

	public FreeTextVariantView(FreeTextVariantViewModel viewModel)
	{
		ViewModel = viewModel;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/FreeText/FreeTextVariantView");
	}

	public static double GetViewMaxHeight() => ViewHeight;
}
