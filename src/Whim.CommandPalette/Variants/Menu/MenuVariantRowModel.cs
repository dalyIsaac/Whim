namespace Whim.CommandPalette;

public record MenuVariantRowModelData(ICommand Command, IKeybind? Keybind = null);

internal record MenuVariantRowModel : IVariantRowModel<MenuVariantRowModelData>
{
	public string Id => Data.Command.Id;

	public string Title => Data.Command.Title;

	public PaletteText FormattedTitle { get; set; }

	public MenuVariantRowModelData Data { get; }

	public MenuVariantRowModel(ICommand command, IKeybind? keybind)
	{
		Data = new MenuVariantRowModelData(command, keybind);
		FormattedTitle = new(command.Title);
	}
}
