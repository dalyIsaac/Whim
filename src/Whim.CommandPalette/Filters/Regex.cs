using System.Text.RegularExpressions;

namespace Whim.CommandPalette;

public static partial class PaletteFilters
{
	/// <summary>
	/// Returns matches of the word compared to the wordToMatchAgainst, where the word is a regular expression.
	/// </summary>
	public static FilterTextMatch[]? MatchesRegex(string word, string wordToMatchAgainst)
	{
		Regex regexp = new(word, RegexOptions.IgnoreCase);

		Match match = regexp.Match(wordToMatchAgainst);
		if (match.Success)
		{
			return [new FilterTextMatch(match.Index, match.Index + match.Length)];
		}

		return null;
	}
}
