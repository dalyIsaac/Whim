namespace Whim.CommandPalette.Tests;

internal class MenuRowStub : IVariantRowControl<CommandItem>
{
	public bool IsUpdated { get; private set; }

	public required IVariantRowModel<CommandItem> Model { get; set; }

	public void Initialize() { }

	public void Update(IVariantRowModel<CommandItem> item)
	{
		Model = item;
		IsUpdated = true;
	}
}
