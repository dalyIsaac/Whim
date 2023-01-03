namespace Whim.CommandPalette;

internal record MenuPaletteItem : IVariantItem<CommandItem>
{
	public string Id => Data.Command.Id;

	public string Title => Data.Command.Title;

	public Text FormattedTitle
	{
		get => throw new System.NotImplementedException();
		set => throw new System.NotImplementedException();
	}

	public CommandItem Data { get; }

	public MenuPaletteItem(CommandItem data)
	{
		Data = data;
	}
}
