using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class RadioButtonRow : UserControl, IVariantRow<SelectOption>
{
	private readonly SelectVariantViewModel _selectVariantViewModel;

	public IVariantModel<SelectOption> Item { get; private set; }

	public RadioButtonRow(SelectVariantViewModel selectVariantViewModel, IVariantModel<SelectOption> item)
	{
		_selectVariantViewModel = selectVariantViewModel;
		Item = item;
		UIElementExtensions.InitializeComponent(
			component: this,
			"Whim.CommandPalette",
			"Variants/Select/RadioButtonRow"
		);
	}

	public void Initialize()
	{
		this.SetTitle(OptionTitle.Inlines);
	}

	public void Update(IVariantModel<SelectOption> item)
	{
		Logger.Debug("Updating with a new item");
		Item = item;
		this.SetTitle(OptionTitle.Inlines);
	}

	private void RadioButton_Click(object _, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		_selectVariantViewModel.VariantRow_OnClick(this);
	}
}
