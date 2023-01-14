using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class RadioButtonRow : UserControl, IVariantRow<SelectOption>
{
	private readonly SelectVariantViewModel _viewModel;

	public IVariantItem<SelectOption> Item { get; private set; }

	public RadioButtonRow(SelectVariantViewModel viewModel, IVariantItem<SelectOption> item)
	{
		_viewModel = viewModel;
		Item = item;
		UIElementExtensions.InitializeComponent(component: this, "Whim.CommandPalette", "Variants/RadioButton/RadioButtonRow");
	}

	public void Initialize()
	{
		this.SetTitle(OptionTitle.Inlines);
	}

	public void Update(IVariantItem<SelectOption> item)
	{
		Logger.Debug("Updating with a new item");
		Item = item;
		this.SetTitle(OptionTitle.Inlines);
	}
}
