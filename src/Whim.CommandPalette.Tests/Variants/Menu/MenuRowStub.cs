namespace Whim.CommandPalette.Tests;

internal class MenuRowStub : IVariantRow<CommandItem>
{
	public bool IsUpdated { get; private set; }

	public required IVariantModel<CommandItem> Item { get; set; }

	public void Initialize() { }

	public void Update(IVariantModel<CommandItem> item)
	{
		Item = item;
		IsUpdated = true;
	}
}
