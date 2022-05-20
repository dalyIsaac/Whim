namespace Whim.CommandPalette;

public class CommandPaletteConfig
{
	internal const string Title = "Whim Command Palette";

	public ICommandPaletteMatcher Matcher { get; set; } = new MostOftenUsedMatcher();
}
