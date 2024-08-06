namespace Whim.CommandPalette;

/// <summary>
/// Utilities for palette filters.
/// </summary>
public static partial class PaletteFilters
{
	/// <summary>
	/// Returns a filter which returns the matches for the first filter which returns a
	/// non-null value.
	/// </summary>
	/// <param name="filters">The filters to iterate over.</param>
	/// <returns></returns>
	public static PaletteFilter Or(params PaletteFilter[] filters) =>
		(word, wordToMatchAgainst) =>
		{
			foreach (PaletteFilter filter in filters)
			{
				FilterTextMatch[]? match = filter(word, wordToMatchAgainst);
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
	public static FilterTextMatch[] Join(FilterTextMatch head, FilterTextMatch[] tail)
	{
		if (tail.Length == 0)
		{
			tail = [head];
		}
		else if (head.End == tail[0].Start)
		{
			tail[0] = new FilterTextMatch(head.Start, tail[0].End);
		}
		else
		{
			tail = [head, .. tail];
		}

		return tail;
	}
}
