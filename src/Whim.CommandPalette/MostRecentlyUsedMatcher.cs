using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Whim.CommandPalette;

/// <summary>
/// MostRecentlyUsedMatcher will return matches in the order of most recently used.
/// </summary>
public class MostRecentlyUsedMatcher : ICommandPaletteMatcher
{
	private static readonly MatcherCommandItemComparer _sorter = new();

	private readonly Dictionary<string, uint> _commandLastExecutionTime = new();

	/// <summary>
	/// The filter to use when matching commands.
	/// </summary>
	public PaletteFilter Filter { get; set; } = PaletteFilters.MatchesFuzzyContiguous;

	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public ICollection<PaletteRowItem> Match(string query, ICollection<CommandItem> commandItems)
	{
		MatcherCommandItem[] matches = new MatcherCommandItem[commandItems.Count];
		int matchCount = GetFilteredItems(query, commandItems, matches);

		if (matchCount == 0)
		{
			// If there are no matches and the query is not empty, return an empty list.
			if (!string.IsNullOrEmpty(query))
			{
				return Array.Empty<PaletteRowItem>();
			}

			// If there are no matches and the query is empty, return the most recently used items.
			matchCount = GetMostRecentlyUsedItems(commandItems, matches);
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

	/// <summary>
	/// Iterates over the given command <paramref name="items"/> and updates the <paramref name="matches"/>
	/// array with the filtered matches, with updated text segments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetFilteredItems(string query, ICollection<CommandItem> items, MatcherCommandItem[] matches)
	{
		int matchCount = 0;

		// Get the matches for the query.
		foreach (CommandItem item in items)
		{
			PaletteFilterTextMatch[]? filterMatches = Filter(query, item.Command.Title);
			if (filterMatches == null)
			{
				continue;
			}

			uint count = _commandLastExecutionTime.GetValueOrDefault<string, uint>(item.Command.Identifier, 0);
			matches[matchCount++] = new MatcherCommandItem()
			{
				Item = item,
				TextSegments = filterMatches,
				Score = count
			};
		}

		return matchCount;
	}

	/// <summary>
	/// Iterates over the given command <paramref name="items"/> and updates the <paramref name="matches"/>
	/// array with the last execution time of each command.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetMostRecentlyUsedItems(ICollection<CommandItem> items, MatcherCommandItem[] matches)
	{
		int matchCount = 0;

		foreach (CommandItem item in items)
		{
			uint lastExecutionTime = _commandLastExecutionTime.TryGetValue(item.Command.Identifier, out uint value)
				? value
				: 0;
			matches[matchCount++] = new MatcherCommandItem()
			{
				Item = item,
				TextSegments = Array.Empty<PaletteFilterTextMatch>(),
				Score = lastExecutionTime
			};
		}

		return matchCount;
	}

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="ICommandPaletteMatcher"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(CommandItem match)
	{
		string id = match.Command.Identifier;
		_commandLastExecutionTime[id] = (uint)DateTime.Now.Ticks;
	}
}
