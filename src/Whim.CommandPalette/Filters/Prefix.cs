using System;

namespace Whim.CommandPalette;

public static partial class PaletteFilters
{
	/// <summary>
	/// Returns prefix matches of the word, compared to the wordToMatchAgainst.
	/// Cases are matched exactly.
	/// </summary>
	public static FilterTextMatch[]? MatchesStrictPrefix(string word, string wordToMatchAgainst)
	{
		return MatchesPrefixCasingBase(word, wordToMatchAgainst, false);
	}

	/// <summary>
	/// Returns exact prefix matches of the word, compared to the wordToMatchAgainst.
	/// Cases are ignored.
	/// </summary>
	public static FilterTextMatch[]? MatchesPrefix(string word, string wordToMatchAgainst)
	{
		return MatchesPrefixCasingBase(word, wordToMatchAgainst, true);
	}

	private static FilterTextMatch[]? MatchesPrefixCasingBase(string word, string wordToMatchAgainst, bool ignoreCase)
	{
		if (string.IsNullOrEmpty(wordToMatchAgainst) || wordToMatchAgainst.Length < word.Length)
		{
			return null;
		}

		bool matches = wordToMatchAgainst.StartsWith(
			word,
			ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
		);
		if (!matches)
		{
			return null;
		}

		return word.Length > 0 ? new[] { new FilterTextMatch(0, word.Length) } : Array.Empty<FilterTextMatch>();
	}
}
