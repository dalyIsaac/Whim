using System.Linq;

namespace Whim.CommandPalette;

/// <summary>
/// Utilities for palette filters.
/// </summary>
public static partial class Filters
{
	/// <summary>
	/// Returns a filter which returns the matches for the first filter which returns a
	/// non-null value.
	/// </summary>
	/// <param name="filters">The filters to iterate over.</param>
	/// <returns></returns>
	public static PaletteFilter Or(params PaletteFilter[] filters) => (word, wordToMatchAgainst) =>
	{
		foreach (PaletteFilter filter in filters)
		{
			PaletteFilterMatch[]? match = filter(word, wordToMatchAgainst);
			if (match != null)
			{
				return match;
			}
		}

		return null;
	};

	/// <summary>
	/// Returns an array with the tail concatenated to the end of the head.
	///
	/// NOTE: This may mutate the provided tail.
	/// </summary>
	/// <param name="head">The match to place at the start of the array of returned matches.</param>
	/// <param name="tail">The matches to place after the head.</param>
	/// <returns></returns>
	public static PaletteFilterMatch[] Join(PaletteFilterMatch head, PaletteFilterMatch[] tail)
	{
		if (tail.Length == 0)
		{
			tail = new[] { head };
		}
		else if (head.End == tail[0].Start)
		{
			tail[0] = new PaletteFilterMatch(head.Start, tail[0].End);
		}
		else
		{
			tail = new[] { head }.Concat(tail).ToArray();
		}

		return tail;
	}
}
