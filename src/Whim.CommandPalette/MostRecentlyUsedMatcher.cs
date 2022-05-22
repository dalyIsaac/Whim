using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

public class MostRecentlyUsedMatcher : ICommandPaletteMatcher
{
	private record struct MatchData(PaletteItem MatchItem, long LastUsed);

	private readonly Dictionary<string, long> _commandExecutionTime = new();

	public IEnumerable<PaletteItem> Match(string query, IEnumerable<Match> items)
	{
		query = query.Trim();
		List<MatchData> filteredItems = new();
		foreach (Match match in items)
		{
			int startIdx = match.Command.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase);
			if (startIdx == -1)
			{
				continue;
			}

			HighlightedText highlightedTitle = new();
			string title = match.Command.Title;

			if (query.Length != 0)
			{
				if (startIdx != 0)
				{
					highlightedTitle.Segments.Add(
						new HighlightedTextSegment(
							title[..startIdx],
							false
						)
					);
				}

				highlightedTitle.Segments.Add(
					new HighlightedTextSegment(
						title.Substring(startIdx, query.Length),
						true
					)
				);

				if (startIdx + query.Length != title.Length)
				{
					highlightedTitle.Segments.Add(
						new HighlightedTextSegment(
							title[(startIdx + query.Length)..],
							false
						)
					);
				}
			}
			else
			{
				highlightedTitle.Segments.Add(new HighlightedTextSegment(title, false));
			}

			// Get the time the command was last executed.
			_commandExecutionTime.TryGetValue(match.Command.Identifier, out long time);
			filteredItems.Add(new MatchData(new PaletteItem(match, highlightedTitle), time));
		}

		// Sort the filtered items by the last time the command was executed.
		filteredItems.Sort((a, b) => -a.LastUsed.CompareTo(b.LastUsed));

		// Return the filtered items.
		foreach (MatchData data in filteredItems)
		{
			yield return data.MatchItem;
		}
	}

	public void OnMatchExecuted(Match match)
	{
		string id = match.Command.Identifier;
		_commandExecutionTime[id] = DateTime.Now.Ticks;
	}
}
