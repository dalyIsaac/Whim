using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class FreeTextVariantControl : IVariantControl
{
	private readonly FreeTextVariantView _control;
	public UIElement Control => _control;

	public IVariantViewModel ViewModel { get; }

	public FreeTextVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		FreeTextVariantViewModel viewModel = new(windowViewModel);
		ViewModel = viewModel;
		_control = new FreeTextVariantView(viewModel);
	}

	public double GetViewMaxHeight() => FreeTextVariantView.GetViewMaxHeight();
}
