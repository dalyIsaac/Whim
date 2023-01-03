using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class FreeTextVariantControl : IVariantControl<FreeTextVariantConfig>
{
	private readonly FreeTextVariantView _control;
	public UIElement Control => _control;

	public IVariantViewModel<FreeTextVariantConfig> ViewModel { get; }

	public FreeTextVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		FreeTextVariantViewModel viewModel = new(windowViewModel);
		ViewModel = viewModel;
		_control = new FreeTextVariantView(viewModel);
	}

	public double GetViewHeight() => _control.GetViewHeight();
}
