using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A matcher is used by the command palette to find commands that match a given input.
/// </summary>
public interface ICommandPaletteMatcher
{
	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public IEnumerable<PaletteItem> Match(
		string query,
		IEnumerable<Match> items
	);

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="ICommandPaletteMatcher"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(Match match);
}
