using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class CheckBoxVariantControl : IVariantControl
{
	private readonly CheckBoxVariantView _control;
	public UIElement Control => _control;

	private readonly SelectVariantViewModel _viewModel;
	public IVariantViewModel ViewModel => _viewModel;

	public CheckBoxVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		_viewModel = new(windowViewModel, true, (item) => new CheckBoxRow(item)) { RowHeight = 24 };
		_control = new CheckBoxVariantView(_viewModel);
	}

	public double GetViewMaxHeight() => _viewModel.GetViewMaxHeight();
}
