namespace Whim.CommandPalette;

internal record SelectVariantItem : IVariantItem<SelectOption>
{
	public string Id => Data.Id;

	public string Title => Data.Title;

	public PaletteText FormattedTitle { get; set; }

	public SelectOption Data { get; }

	public SelectVariantItem(SelectOption data)
	{
		Data = data;
		FormattedTitle = data.Title;
	}
}
