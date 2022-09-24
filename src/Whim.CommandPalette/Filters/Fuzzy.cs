namespace Whim.CommandPalette;

public static partial class Filters
{
	/// <summary>
	/// Filter which returns contiguous matches of a word, compared to a given word to match against.
	/// </summary>
	public static readonly PaletteFilter FuzzyContiguousFilter = Or(MatchesPrefix, MatchesCamelCase, MatchesContiguousSubString);

	/// <summary>
	/// Filter which returns separate matches of a word, compared to a given word to match against.
	/// </summary>
	public static readonly PaletteFilter FuzzySeparateFilter = Or(MatchesPrefix, MatchesCamelCase, MatchesSubString);

	/// <summary>
	/// Filter which returns prefix, separate words, and contiguous string matches of a word, compared to a given word to match against.
	/// </summary>
	public static readonly PaletteFilter WordFilter = Or(MatchesPrefix, MatchesWordsSeparate, MatchesContiguousSubString);

	/// <summary>
	/// Returns fuzzy contiguous matches of the word, compared to the wordToMatchAgainst.
	/// Handles wildcards, using <see cref="MatchesRegex"/>.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesFuzzyContiguous(string word, string wordToMatchAgainst)
	{
		PaletteFilterMatch[]? regexResult = MatchesRegex(word, wordToMatchAgainst);
		if (regexResult is not null)
		{
			return regexResult;
		}

		return FuzzyContiguousFilter(word, wordToMatchAgainst);
	}

	/// <summary>
	/// Returns fuzzy separate matches of the word, compared to the wordToMatchAgainst.
	/// Handles wildcards, using <see cref="MatchesRegex"/>.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesFuzzySeparate(string word, string wordToMatchAgainst)
	{
		PaletteFilterMatch[]? regexResult = MatchesRegex(word, wordToMatchAgainst);
		if (regexResult is not null)
		{
			return regexResult;
		}

		return FuzzySeparateFilter(word, wordToMatchAgainst);
	}
}
