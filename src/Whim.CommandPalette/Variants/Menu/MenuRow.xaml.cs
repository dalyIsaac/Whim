using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class MenuRow : UserControl, IVariantRow<CommandItem>
{
	public static double MenuRowHeight => 24;

	public IVariantItem<CommandItem> Item { get; private set; }

	public MenuRow(IVariantItem<CommandItem> item)
	{
		Item = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteRow");
	}

	public void Initialize()
	{
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	public void Update(IVariantItem<CommandItem> item)
	{
		Logger.Debug("Updating with a new item");
		Item = item;
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	private void SetKeybinds()
	{
		Logger.Debug("Setting keybinds");

		if (Item.Data.Keybind is not null)
		{
			CommandKeybind.Text = Item.Data.Keybind.ToString();
			CommandKeybindBorder.Visibility = Visibility.Visible;
		}
		else
		{
			CommandKeybindBorder.Visibility = Visibility.Collapsed;
		}
	}
}
