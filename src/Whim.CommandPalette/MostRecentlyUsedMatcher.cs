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
			match.Title.Segments.Clear();

			int startIdx = match.Command.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase);
			if (startIdx == -1)
			{
				continue;
			}

			if (query.Length != 0)
			{
				if (startIdx != 0)
				{
					match.Title.Segments.Add(
						new HighlightedTextSegment(
							match.Command.Title[..startIdx],
							false
						)
					);
				}

				match.Title.Segments.Add(
					new HighlightedTextSegment(
						match.Command.Title.Substring(startIdx, query.Length),
						true
					)
				);

				if (startIdx + query.Length != match.Command.Title.Length)
				{
					match.Title.Segments.Add(
						new HighlightedTextSegment(
							match.Command.Title[(startIdx + query.Length)..],
							false
						)
					);
				}
			}
			else
			{
				match.Title.Segments.Add(new HighlightedTextSegment(match.Command.Title, false));
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
