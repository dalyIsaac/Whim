using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class CheckBoxRow : UserControl, IVariantRow<SelectOption>
{
	public IVariantItem<SelectOption> Item { get; private set; }

	public CheckBoxRow(IVariantItem<SelectOption> item)
	{
		Item = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/Select/CheckBoxRow");
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
