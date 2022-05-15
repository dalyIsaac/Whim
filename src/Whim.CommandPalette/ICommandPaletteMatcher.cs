using System.Collections.Generic;

namespace Whim.CommandPalette;

public interface ICommandPaletteMatcher
{
	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public IEnumerable<CommandPaletteMatch> Match(
		string query,
		IEnumerable<CommandPaletteMatch> items,
		IConfigContext configContext,
		CommandPalettePlugin plugin
	);
}
