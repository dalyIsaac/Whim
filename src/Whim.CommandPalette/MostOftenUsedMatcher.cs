using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// MostOftenUsedMatcher will return matches in the order of the most often used.
/// </summary>
public class MostOftenUsedMatcher : ICommandPaletteMatcher
{
	private record struct MatchData(CommandPaletteMatch Match, uint Count);

	private readonly Dictionary<string, uint> _commandExecutionCount = new();

	public IEnumerable<CommandPaletteMatch> Match(
		string query,
		IEnumerable<CommandPaletteMatch> items
	)
	{
		query = query.Trim();
		List<MatchData> filteredItems = new();
		foreach (CommandPaletteMatch match in items)
		{
			if (!match.Command.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			// Get the number of times the command has been executed.
			_commandExecutionCount.TryGetValue(match.Command.Identifier, out uint count);
			filteredItems.Add(new(match, count));
		}

		// Sort the filtered items by the number of times the command has been executed.
		filteredItems.Sort((a, b) => -a.Count.CompareTo(b.Count));

		// Return the filtered items.
		foreach (MatchData data in filteredItems)
		{
			yield return data.Match;
		}
	}

	public void OnMatchExecuted(CommandPaletteMatch match)
	{
		string id = match.Command.Identifier;
		_commandExecutionCount.TryGetValue(id, out uint count);
		_commandExecutionCount[id] = count + 1;
	}
}
