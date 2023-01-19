using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

internal class SelectVariantControl : IVariantControl
{
	private readonly SelectVariantView _control;
	public UIElement Control => _control;

	private readonly SelectVariantViewModel _viewModel;
	public IVariantViewModel ViewModel => _viewModel;

	public SelectVariantControl(ICommandPaletteWindowViewModel windowViewModel)
	{
		_viewModel = new(windowViewModel, true, SelectRowFactory) { RowHeight = 24 };
		_control = new SelectVariantView(_viewModel);
	}

	private IVariantRow<SelectOption> SelectRowFactory(IVariantItem<SelectOption> item, SelectVariantConfig config)
	{
		if (config.AllowMultiSelect)
		{
			return new CheckBoxRow(_viewModel, item);
		}
		else
		{
			return new RadioButtonRow(_viewModel, item);
		}
	}

	public double GetViewMaxHeight() => _viewModel.GetViewMaxHeight();
}
