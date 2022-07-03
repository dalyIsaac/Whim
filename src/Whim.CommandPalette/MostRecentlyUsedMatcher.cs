using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// MostRecentlyUsedMatcher will return matches in the order of most recently used.
/// </summary>
public class MostRecentlyUsedMatcher : ICommandPaletteMatcher
{
	private record struct MatchData(PaletteItem MatchItem, long LastUsed);

	private readonly Dictionary<string, long> _commandExecutionTime = new();

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void OnMatchExecuted(Match match)
	{
		string id = match.Command.Identifier;
		_commandExecutionTime[id] = DateTime.Now.Ticks;
	}
}
