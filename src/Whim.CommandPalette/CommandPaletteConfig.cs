using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
/// </summary>
public delegate List<CommandPaletteMatch> Matcher(
	string query,
	IEnumerable<(ICommand, IKeybind?)> items,
	IConfigContext configContext
);

public class CommandPaletteConfig
{
	internal const string Title = "Whim Command Palette";

	public Matcher Matcher { get; set; } = DefaultMatcher;
}
