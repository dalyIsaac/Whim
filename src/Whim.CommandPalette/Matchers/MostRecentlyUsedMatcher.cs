using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Whim.CommandPalette;

/// <summary>
/// MostRecentlyUsedMatcher will return matches in the order of most recently used.
/// </summary>
public class MostRecentlyUsedMatcher<T> : IMatcher<T>
{
	private static readonly MatcherItemComparer<T> _sorter = new();

	private readonly Dictionary<string, uint> _commandLastExecutionTime = new();

	/// <summary>
	/// The filter to use when matching commands.
	/// </summary>
	public PaletteFilter Filter { get; set; } = PaletteFilters.MatchesFuzzyContiguous;

	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public IEnumerable<IVariantItem<T>> Match(string query, IReadOnlyList<IVariantItem<T>> inputItems)
	{
		MatcherItem<T>[] matches = new MatcherItem<T>[inputItems.Count];
		int matchCount = GetFilteredItems(query, inputItems, matches);

		if (matchCount == 0)
		{
			// If there are no matches and the query is not empty, return an empty list.
			if (!string.IsNullOrEmpty(query))
			{
				return Array.Empty<IVariantItem<T>>();
			}

			// If there are no matches and the query is empty, return the most recently used items.
			matchCount = GetMostRecentlyUsedItems(inputItems, matches);
		}

		// Sort the matches.
		Array.Sort(matches, 0, matchCount, _sorter);

		// Get the IPaletteItem<T> from the matches.
		IVariantItem<T>[] matchedItems = new IVariantItem<T>[matchCount];
		for (int i = 0; i < matchCount; i++)
		{
			MatcherItem<T> current = matches[i];
			current.FormatTitle();
			matchedItems[i] = current.Item;
		}
		return matchedItems;
	}

	/// <summary>
	/// Iterates over the given command <paramref name="items"/> and updates the <paramref name="matches"/>
	/// array with the filtered matches, with updated text segments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetFilteredItems(string query, IEnumerable<IVariantItem<T>> items, MatcherItem<T>[] matches)
	{
		int matchCount = 0;

		// Get the matches for the query.
		foreach (IVariantItem<T> item in items)
		{
			FilterTextMatch[]? filterMatches = Filter(query, item.Title);
			if (filterMatches == null)
			{
				continue;
			}

			uint count = _commandLastExecutionTime.GetValueOrDefault<string, uint>(item.Id, 0);
			matches[matchCount++] = new MatcherItem<T>()
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
	internal int GetMostRecentlyUsedItems(IEnumerable<IVariantItem<T>> items, MatcherItem<T>[] matches)
	{
		int matchCount = 0;

		foreach (IVariantItem<T> item in items)
		{
			uint lastExecutionTime = _commandLastExecutionTime.TryGetValue(item.Id, out uint value) ? value : 0;
			matches[matchCount++] = new MatcherItem<T>()
			{
				Item = item,
				TextSegments = Array.Empty<FilterTextMatch>(),
				Score = lastExecutionTime
			};
		}

		return matchCount;
	}

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="IMatcher{T}"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(IVariantItem<T> item)
	{
		_commandLastExecutionTime[item.Id] = (uint)DateTime.Now.Ticks;
	}
}
