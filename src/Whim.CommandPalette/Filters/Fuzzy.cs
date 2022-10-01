namespace Whim.CommandPalette;

public static partial class PaletteFilters
{
	/// <summary>
	/// Filter which returns contiguous matches of a word, compared to a given word to match against.
	/// </summary>
	public static readonly PaletteFilter MatchesFuzzyContiguous = Or(MatchesRegex, MatchesPrefix, MatchesCamelCase, MatchesContiguousSubString);

	/// <summary>
	/// Filter which returns separate matches of a word, compared to a given word to match against.
	/// </summary>
	public static readonly PaletteFilter MatchesFuzzySeparate = Or(MatchesRegex, MatchesPrefix, MatchesCamelCase, MatchesSubString);

	/// <summary>
	/// Filter which returns prefix, separate words, and contiguous string matches of a word, compared to a given word to match against.
	/// </summary>
	public static readonly PaletteFilter MatchesWords = Or(MatchesPrefix, MatchesWordsSeparate, MatchesContiguousSubString);
}
