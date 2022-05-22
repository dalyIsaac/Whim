using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

public sealed partial class PaletteKeybindItem : UserControl
{
	public string KeybindItem { get => _keybindItem.Text; set => _keybindItem.Text = value; }

	public PaletteKeybindItem(string key)
	{
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteItems/PaletteKeybindItem");
		KeybindItem = key;
	}
}
