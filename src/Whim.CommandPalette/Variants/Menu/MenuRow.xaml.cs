using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class MenuRow : UserControl, IVariantRowControl<CommandItem>
{
	public static double MenuRowHeight => 24;

	public IVariantRowModel<CommandItem> Model { get; private set; }

	public MenuRow(IVariantRowModel<CommandItem> item)
	{
		Model = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/Menu/MenuRow");
	}

	public void Initialize()
	{
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	public void Update(IVariantRowModel<CommandItem> item)
	{
		Logger.Debug("Updating with a new item");
		Model = item;
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	private void SetKeybinds()
	{
		Logger.Debug("Setting keybinds");

		if (Model.Data.Keybind is not null)
		{
			CommandKeybind.Text = Model.Data.Keybind.ToString();
			CommandKeybindBorder.Visibility = Visibility.Visible;
		}
		else
		{
			CommandKeybindBorder.Visibility = Visibility.Collapsed;
		}
	}
}
