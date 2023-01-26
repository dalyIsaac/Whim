namespace Whim.CommandPalette.Tests;

internal class MenuRowStub : IVariantRow<CommandItem>
{
	public bool IsUpdated { get; private set; }

	public required IVariantItem<CommandItem> Item { get; set; }

	public void Initialize() { }

	public void Update(IVariantItem<CommandItem> item)
	{
		Item = item;
		IsUpdated = true;
	}
}
