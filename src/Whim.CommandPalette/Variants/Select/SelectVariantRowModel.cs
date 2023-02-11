namespace Whim.CommandPalette;

internal record SelectVariantRowModel : IVariantRowModel<SelectOption>
{
	public string Id => Data.Id;

	public string Title => Data.Title;

	public PaletteText FormattedTitle { get; set; }

	public SelectOption Data { get; }

	public SelectVariantRowModel(SelectOption data)
	{
		Data = data;
		FormattedTitle = data.Title;
	}
}
