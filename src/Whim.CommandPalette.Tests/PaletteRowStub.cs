namespace Whim.CommandPalette.Tests;

internal class PaletteRowStub : IPaletteRow
{
	public bool IsUpdated { get; private set; }

	public required PaletteRowItem Model { get; set; }

	public void Initialize() { }

	public void Update(PaletteRowItem item)
	{
		Model = item;
		IsUpdated = true;
	}
}
