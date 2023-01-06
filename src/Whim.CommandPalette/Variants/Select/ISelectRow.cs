namespace Whim.CommandPalette;

internal interface ISelectRow
{
	public IVariantItem<SelectOption> Item { get; }

	public void Initialize();

	public void Update(IVariantItem<SelectOption> item);
}
