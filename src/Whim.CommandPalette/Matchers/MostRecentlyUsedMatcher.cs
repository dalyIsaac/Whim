using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Whim.CommandPalette;

/// <summary>
/// MostRecentlyUsedMatcher will return matches in the order of most recently used.
/// </summary>
public class MostRecentlyUsedMatcher<T> : IMatcher<T>
{
	private static readonly MatcherItemComparer<T> _sorter = new();

	internal readonly Dictionary<string, uint> _commandLastExecutionTime = new();

	/// <summary>
	/// The filter to use when matching commands.
	/// </summary>
	public PaletteFilter Filter { get; set; } = PaletteFilters.MatchesFuzzyContiguous;

	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public IEnumerable<MatcherResult<T>> Match(string query, IReadOnlyList<IVariantRowModel<T>> inputItems)
	{
		MatcherResult<T>[] matches = new MatcherResult<T>[inputItems.Count];
		int matchCount = GetFilteredItems(query, inputItems, matches);

		if (matchCount == 0)
		{
			// If there are no matches and the query is not empty, return an empty list.
			if (!string.IsNullOrEmpty(query))
			{
				return Array.Empty<MatcherResult<T>>();
			}

			// If there are no matches and the query is empty, return the most recently used items.
			matchCount = GetMostRecentlyUsedItems(inputItems, matches);
		}

		// Sort the matches.
		Array.Sort(matches, 0, matchCount, _sorter);

		// Return the matches.
		return matches.Take(matchCount);
	}

	/// <summary>
	/// Iterates over the given command <paramref name="items"/> and updates the <paramref name="matches"/>
	/// array with the filtered matches, with updated text segments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetFilteredItems(string query, IEnumerable<IVariantRowModel<T>> items, MatcherResult<T>[] matches)
	{
		int matchCount = 0;

		// Get the matches for the query.
		foreach (IVariantRowModel<T> item in items)
		{
			FilterTextMatch[]? filterMatches = Filter(query, item.Title);
			if (filterMatches == null)
			{
				continue;
			}

			uint count = _commandLastExecutionTime.GetValueOrDefault<string, uint>(item.Id, 0);
			matches[matchCount++] = new MatcherResult<T>(item, filterMatches, count);
		}

		return matchCount;
	}

	/// <summary>
	/// Iterates over the given command <paramref name="items"/> and updates the <paramref name="matches"/>
	/// array with the last execution time of each command.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetMostRecentlyUsedItems(IEnumerable<IVariantRowModel<T>> items, MatcherResult<T>[] matches)
	{
		int matchCount = 0;

		foreach (IVariantRowModel<T> item in items)
		{
			uint lastExecutionTime = _commandLastExecutionTime.TryGetValue(item.Id, out uint value) ? value : 0;
			matches[matchCount++] = new MatcherResult<T>(item, Array.Empty<FilterTextMatch>(), lastExecutionTime);
		}

		return matchCount;
	}

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="IMatcher{T}"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(IVariantRowModel<T> item)
	{
		_commandLastExecutionTime[item.Id] = (uint)DateTime.Now.Ticks;
	}

	/// <inheritdoc/>
	public void LoadState(JsonElement state)
	{
		_commandLastExecutionTime.Clear();

		if (state.ValueKind != JsonValueKind.Object)
		{
			return;
		}

		foreach (JsonProperty property in state.EnumerateObject())
		{
			if (property.Value.ValueKind != JsonValueKind.Number)
			{
				continue;
			}

			_commandLastExecutionTime[property.Name] = (uint)property.Value.GetUInt64();
		}
	}

	/// <inheritdoc/>
	public JsonElement? SaveState()
	{
		if (_commandLastExecutionTime.Count == 0)
		{
			return null;
		}

		return JsonSerializer.SerializeToElement(_commandLastExecutionTime);
	}
}
