using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class SelectRow : UserControl, IVariantRow<SelectOption>
{
	public static double SelectRowHeight => 24;

	public IVariantItem<SelectOption> Item { get; private set; }

	public SelectRow(IVariantItem<SelectOption> item)
	{
		Item = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteRow");
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

	private void CheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
