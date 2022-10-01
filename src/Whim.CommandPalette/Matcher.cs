using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Whim.CommandPalette;

/// <summary>
/// A matcher is used by the command palette to find commands that match a given input.
/// </summary>
public class Matcher
{
	private static readonly PaletteItemSortPayloadSorter _sorter = new();

	private readonly Dictionary<string, uint> _commandLastExecutionTime = new();

	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public ICollection<PaletteRowItem> Match(string query, ICollection<PaletteItem> items)
	{
		PaletteItemSortPayload[] matches = new PaletteItemSortPayload[items.Count];
		int matchCount = GetFilteredItems(query, items, matches);

		// If there are no matches, return the most recently used items.
		if (matchCount == 0)
		{
			matchCount = GetMostRecentlyUsedItems(items, matches);
		}

		// Sort the matches.
		Array.Sort(matches, 0, matchCount, _sorter);

		// Convert the matches to PaletteRowItems.
		PaletteRowItem[] rowItems = new PaletteRowItem[matchCount];
		for (int i = 0; i < matchCount; i++)
		{
			rowItems[i] = matches[i].ToRowItem();
		}

		return rowItems;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetFilteredItems(
		string query,
		ICollection<PaletteItem> items,
		PaletteItemSortPayload[] matches
	)
	{
		int matchCount = 0;

		// Get the matches for the query.
		foreach (PaletteItem item in items)
		{
			PaletteFilterMatch[]? filterMatches = Filters.MatchesFuzzyContiguous(query, item.Command.Title);
			if (filterMatches == null)
			{
				continue;
			}

			uint count = _commandLastExecutionTime.TryGetValue(item.Command.Identifier, out uint value) ? value : 0;
			matches[matchCount++] = new PaletteItemSortPayload(item, filterMatches, count);
		}

		return matchCount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetMostRecentlyUsedItems(
		ICollection<PaletteItem> items,
		PaletteItemSortPayload[] matches
	)
	{
		int matchCount = 0;

		foreach (PaletteItem item in items)
		{
			uint count = _commandLastExecutionTime.TryGetValue(item.Command.Identifier, out uint value) ? value : 0;
			matches[matchCount++] = new PaletteItemSortPayload(item, Array.Empty<PaletteFilterMatch>(), count);
		}

		return matchCount;
	}

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="ICommandPaletteMatcher"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(PaletteItem match)
	{
		string id = match.Command.Identifier;
		_commandLastExecutionTime[id] = (uint)DateTime.Now.Ticks;
	}
}
