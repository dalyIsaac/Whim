using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A command item, the text matches from the query, and the score of the item for the
/// <see cref="ICommandPaletteMatcher"/>.
/// </summary>
internal record MatcherCommandItem
{
	public CommandItem Item { get; }
	public PaletteFilterTextMatch[] TextSegments { get; }
	public uint Score { get; }

	public MatcherCommandItem(CommandItem item, PaletteFilterTextMatch[] textSegments, uint score)
	{
		Item = item;
		TextSegments = textSegments;
		Score = score;
	}

	public PaletteRowItem ToRowItem()
	{
		PaletteRowText title = new();
		ReadOnlySpan<char> rawTitle = Item.Command.Title.AsSpan();

		int start = 0;
		foreach (PaletteFilterTextMatch match in TextSegments)
		{
			if (start < match.Start)
			{
				title.Segments.Add(new TextSegment(rawTitle[start..match.Start].ToString(), false));
				start = match.Start;
			}

			title.Segments.Add(new TextSegment(rawTitle[match.Start..match.End].ToString(), true));
			start = match.End;
		}

		if (start < rawTitle.Length)
		{
			title.Segments.Add(new TextSegment(rawTitle[start..].ToString(), false));
		}

		return new PaletteRowItem(Item, title);
	}
}

/// <summary>
/// Defines a method to compare <see cref="MatcherCommandItem"/>.
/// A higher score will be sorted first. For equal scores, the title will be sorted normally.
/// </summary>
internal class MatcherCommandItemComparer : IComparer<MatcherCommandItem>
{
	/// <inheritdoc />
	public int Compare(MatcherCommandItem? x, MatcherCommandItem? y)
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
		return string.Compare(x.Item.Command.Title, y.Item.Command.Title, StringComparison.Ordinal);
	}
}
