namespace Whim.CommandPalette;

internal record MenuVariantRowModel : IVariantRowModel<CommandItem>
{
	public string Id => Data.Command.Id;

	public string Title => Data.Command.Title;

	public PaletteText FormattedTitle { get; set; }

	public CommandItem Data { get; }

	public MenuVariantRowModel(CommandItem data)
	{
		Data = data;
		FormattedTitle = data.Command.Title;
	}
}
