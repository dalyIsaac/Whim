using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class CheckBoxVariantControl : IVariantControl
{
	private readonly CheckBoxVariantView _control;
	public UIElement Control => _control;

	public IVariantViewModel ViewModel { get; }

	public CheckBoxVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		SelectVariantViewModel viewModel = new(windowViewModel, true, (item) => new CheckBoxRow(item));
		ViewModel = viewModel;
		_control = new CheckBoxVariantView(viewModel);
	}

	public double GetViewMaxHeight() => _control.GetViewMaxHeight();
}
