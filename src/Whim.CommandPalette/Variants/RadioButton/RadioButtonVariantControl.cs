using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class RadioButtonVariantControl : IVariantControl
{
	private readonly RadioButtonVariantView _control;
	public UIElement Control => _control;

	public IVariantViewModel ViewModel { get; }

	public RadioButtonVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		SelectVariantViewModel viewModel = new(windowViewModel, false, (item) => new RadioButtonRow(item));
		ViewModel = viewModel;
		_control = new RadioButtonVariantView(viewModel);
	}

	public double GetViewMaxHeight() => _control.GetViewMaxHeight();
}
