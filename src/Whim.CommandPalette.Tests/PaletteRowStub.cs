namespace Whim.CommandPalette.Tests;

internal class PaletteRowStub : IPaletteRow
{
	public required PaletteRowItem Model { get; set; }

	public void Initialize() { }

	public void Update(PaletteRowItem item)
	{
		Model = item;
	}
}
