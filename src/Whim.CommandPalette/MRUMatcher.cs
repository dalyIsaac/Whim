using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// MRUMatcher will return matches in the order of the most recently used.
/// </summary>
public class MRUMatcher : ICommandPaletteMatcher
{
	public IEnumerable<CommandPaletteMatch> Match(
		string query,
		IEnumerable<CommandPaletteMatch> items,
		IConfigContext configContext,
		CommandPalettePlugin plugin
	)
	{
		// Filter out the items which cannot be executed, or are not a match.
		List<(int, CommandPaletteMatch)> filteredItems = new();
		foreach (CommandPaletteMatch match in items)
		{
			int startIdx = match.Command.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase);
			if (startIdx == -1)
			{
				continue;
			}

			filteredItems.Add((startIdx, match));
		}

		// Sort the filtered items by the start index of the match.
		filteredItems.Sort((a, b) => a.Item1.CompareTo(b.Item1));

		// Return the filtered items.
		foreach ((int _, CommandPaletteMatch match) in filteredItems)
		{
			yield return match;
		}
	}
}
