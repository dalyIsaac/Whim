using System;

namespace Whim.CommandPalette;

public static partial class PaletteFilters
{
	/// <summary>
	/// Returns prefix matches of the word, compared to the wordToMatchAgainst.
	/// Cases are matched exactly.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesStrictPrefix(string word, string wordToMatchAgainst)
	{
		return MatchesPrefixCasingBase(word, wordToMatchAgainst, false);
	}

	/// <summary>
	/// Returns exact prefix matches of the word, compared to the wordToMatchAgainst.
	/// Cases are ignored.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesPrefix(string word, string wordToMatchAgainst)
	{
		return MatchesPrefixCasingBase(word, wordToMatchAgainst, true);
	}

	private static PaletteFilterMatch[]? MatchesPrefixCasingBase(string word, string wordToMatchAgainst, bool ignoreCase)
	{
		if (string.IsNullOrEmpty(wordToMatchAgainst) || wordToMatchAgainst.Length < word.Length)
		{
			return null;
		}

		bool matches =
		wordToMatchAgainst.StartsWith(word, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		if (!matches)
		{
			return null;
		}

		return word.Length > 0 ? new[] { new PaletteFilterMatch(0, word.Length) } : Array.Empty<PaletteFilterMatch>();
	}
}
