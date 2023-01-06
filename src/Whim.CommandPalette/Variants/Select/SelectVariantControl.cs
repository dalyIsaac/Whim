using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class SelectVariantControl : IVariantControl
{
	private readonly SelectVariantView _control;
	public UIElement Control => _control;

	public IVariantViewModel ViewModel { get; }

	public SelectVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		SelectVariantViewModel viewModel = new(windowViewModel);
		ViewModel = viewModel;
		_control = new SelectVariantView(viewModel);
	}

	public double GetViewMaxHeight() => _control.GetViewMaxHeight();
}
