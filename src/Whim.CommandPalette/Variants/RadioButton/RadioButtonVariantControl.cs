using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class RadioButtonVariantControl : IVariantControl
{
	private readonly RadioButtonVariantView _control;
	public UIElement Control => _control;

	private readonly SelectVariantViewModel _viewModel;
	public IVariantViewModel ViewModel => _viewModel;

	public RadioButtonVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		_viewModel = new(windowViewModel, false, RadioButtonRowFactory) { RowHeight = 24 };
		_control = new RadioButtonVariantView(_viewModel);
	}

	private RadioButtonRow RadioButtonRowFactory(IVariantItem<SelectOption> item) => new(item);

	public double GetViewMaxHeight() => _viewModel.GetViewMaxHeight();
}
