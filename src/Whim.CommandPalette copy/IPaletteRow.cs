namespace Whim.CommandPalette;

internal interface IPaletteRow
{
	public PaletteRowItem Model { get; }

	public void Initialize();

	public void Update(PaletteRowItem item);
}
