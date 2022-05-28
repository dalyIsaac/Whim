using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// MostOftenUsedMatcher will return matches in the order of the most often used.
/// </summary>
public class MostOftenUsedMatcher : ICommandPaletteMatcher
{
	private record struct MatchData(PaletteItem MatchItem, uint Count);

	private readonly Dictionary<string, uint> _commandExecutionCount = new();

	public IEnumerable<PaletteItem> Match(
		string query,
		IEnumerable<Match> items
	)
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

			// Add the match to the list of matches, and highlight the matching text.
			HighlightedText highlightedTitle = new();
			string title = match.Command.Title;

			// If the query is empty, add the entire title as a single segment.
			if (query.Length == 0)
			{
				highlightedTitle.Segments.Add(new HighlightedTextSegment(title, false));
			}
			else
			{
				// Add the text from the start of the title to the start of the query.
				if (startIdx != 0)
				{
					highlightedTitle.Segments.Add(
						new HighlightedTextSegment(
							title[..startIdx],
							false
						)
					);
				}

				// Add the highlighted query text.
				highlightedTitle.Segments.Add(
					new HighlightedTextSegment(
						title.Substring(startIdx, query.Length),
						true
					)
				);

				// Add the text from the end of the query to the end of the title.
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

			// Get the number of times the command has been executed.
			_commandExecutionCount.TryGetValue(match.Command.Identifier, out uint count);
			filteredItems.Add(new MatchData(new PaletteItem(match, highlightedTitle), count));
		}

		// Sort the filtered items by the number of times the command has been executed.
		filteredItems.Sort((a, b) => -a.Count.CompareTo(b.Count));

		// Return the filtered items.
		foreach (MatchData data in filteredItems)
		{
			yield return data.MatchItem;
		}
	}

	public void OnMatchExecuted(Match match)
	{
		string id = match.Command.Identifier;
		_commandExecutionCount.TryGetValue(id, out uint count);
		_commandExecutionCount[id] = count + 1;
	}
}
