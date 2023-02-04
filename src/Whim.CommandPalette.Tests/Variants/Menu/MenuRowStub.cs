namespace Whim.CommandPalette.Tests;

internal class MenuRowStub : IVariantRow<CommandItem>
{
	public bool IsUpdated { get; private set; }

	public required IVariantRowModel<CommandItem> Item { get; set; }

	public void Initialize() { }

	public void Update(IVariantRowModel<CommandItem> item)
	{
		Item = item;
		IsUpdated = true;
	}
}
