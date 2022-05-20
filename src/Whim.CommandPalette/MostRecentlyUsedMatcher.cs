using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

public class MostRecentlyUsedMatcher : ICommandPaletteMatcher
{
	private record struct MatchData(CommandPaletteMatch Match, long LastUsed);

	private readonly Dictionary<string, long> _commandExecutionTime = new();

	public IEnumerable<CommandPaletteMatch> Match(string query, IEnumerable<CommandPaletteMatch> items)
	{
		query = query.Trim();
		List<MatchData> filteredItems = new();
		foreach (CommandPaletteMatch match in items)
		{
			if (!match.Command.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			// Get the time the command was last executed.
			_commandExecutionTime.TryGetValue(match.Command.Identifier, out long time);
			filteredItems.Add(new(match, time));
		}

		// Sort the filtered items by the last time the command was executed.
		filteredItems.Sort((a, b) => -a.LastUsed.CompareTo(b.LastUsed));

		// Return the filtered items.
		foreach (MatchData data in filteredItems)
		{
			yield return data.Match;
		}
	}

	public void OnMatchExecuted(CommandPaletteMatch match)
	{
		string id = match.Command.Identifier;
		_commandExecutionTime[id] = DateTime.Now.Ticks;
	}
}
