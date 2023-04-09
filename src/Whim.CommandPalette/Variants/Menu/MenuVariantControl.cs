using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class MenuVariantControl : IVariantControl
{
	private readonly MenuVariantView _control;
	public UIElement Control => _control;

	public IVariantViewModel ViewModel { get; }

	public MenuVariantControl(IContext context, ICommandPaletteWindowViewModel windowViewModel)
	{
		MenuVariantViewModel viewModel = new(context, windowViewModel);
		ViewModel = viewModel;
		_control = new MenuVariantView(viewModel);
	}

	public double GetViewMaxHeight() => _control.GetViewMaxHeight();
}
