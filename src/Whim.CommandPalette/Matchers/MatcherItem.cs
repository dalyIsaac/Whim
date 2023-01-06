using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A command item, the text matches from the query, and the score of the item for the
/// <see cref="IMatcher{T}"/>.
/// </summary>
internal record MatcherItem<T>
{
	public required IVariantItem<T> Item { get; init; }
	public required FilterTextMatch[] TextSegments { get; init; }
	public required uint Score { get; init; }

	/// <summary>
	/// Formats the title of the <see cref="Item"/> with the <see cref="TextSegments"/>.
	/// </summary>
	public void FormatTitle()
	{
		PaletteText formattedTitle = new();
		ReadOnlySpan<char> rawTitle = Item.Title.AsSpan();

		int start = 0;
		foreach (FilterTextMatch match in TextSegments)
		{
			if (start < match.Start)
			{
				formattedTitle.Segments.Add(new PaletteTextSegment(rawTitle[start..match.Start].ToString(), false));
				start = match.Start;
			}

			formattedTitle.Segments.Add(new PaletteTextSegment(rawTitle[match.Start..match.End].ToString(), true));
			start = match.End;
		}

		if (start < rawTitle.Length)
		{
			formattedTitle.Segments.Add(new PaletteTextSegment(rawTitle[start..].ToString(), false));
		}

		Item.FormattedTitle = formattedTitle;
	}
}

/// <summary>
/// Defines a method to compare <see cref="MatcherItem{T}"/>.
/// A higher score will be sorted first. For equal scores, the title will be sorted normally.
/// </summary>
internal class MatcherItemComparer<T> : IComparer<MatcherItem<T>>
{
	/// <inheritdoc />
	public int Compare(MatcherItem<T>? x, MatcherItem<T>? y)
	{
		// We throw here because it should never happen.
		if (x is null)
		{
			throw new ArgumentNullException(nameof(x));
		}
		else if (y is null)
		{
			throw new ArgumentNullException(nameof(y));
		}

		// Sort by the last used time.
		if (x.Score > y.Score)
		{
			return -1;
		}
		else if (x.Score < y.Score)
		{
			return 1;
		}

		// Sort by alphabetical order.
		return string.Compare(x.Item.Title, y.Item.Title, StringComparison.Ordinal);
	}
}
