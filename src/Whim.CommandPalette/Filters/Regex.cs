using System.Text.RegularExpressions;

namespace Whim.CommandPalette;

public static partial class Filters
{
	/// <summary>
	/// Returns matches of the word compared to the wordToMatchAgainst, where the word is a regular expression.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesRegex(string word, string wordToMatchAgainst)
	{
		Regex regexp = new(word, RegexOptions.IgnoreCase);

		Match match = regexp.Match(wordToMatchAgainst);
		if (match.Success)
		{
			return new[] { new PaletteFilterMatch(match.Index, match.Index + match.Length) };
		}

		return null;
	}
}
