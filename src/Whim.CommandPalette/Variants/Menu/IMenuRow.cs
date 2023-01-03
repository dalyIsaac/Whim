namespace Whim.CommandPalette;

internal interface IMenuRow
{
	public IVariantItem<CommandItem> Item { get; }

	public void Initialize();

	public void Update(IVariantItem<CommandItem> item);
}
