using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class RadioButtonRow : UserControl, IVariantRow<SelectOption>
{
	public static double RadioButtonRowHeight => 24;

	public IVariantItem<SelectOption> Item { get; private set; }

	public RadioButtonRow(IVariantItem<SelectOption> item)
	{
		Item = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/RadioButton/RadioButtonRow");
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

	private void RadioButton_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		Item.Data.IsSelected = true;
		// TODO
	}
}
