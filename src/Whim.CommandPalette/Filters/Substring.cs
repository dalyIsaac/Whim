using System;

namespace Whim.CommandPalette;

public static partial class PaletteFilters
{
	/// <summary>
	/// Returns the match of the word, where the word is a contiguous substring of the wordToMatchAgainst.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesContiguousSubString(string word, string wordToMatchAgainst)
	{
		int index = wordToMatchAgainst.IndexOf(word, StringComparison.OrdinalIgnoreCase);
		if (index == -1)
		{
			return null;
		}

		return new[] { new PaletteFilterMatch(index, index + word.Length) };
	}

	/// <summary>
	/// Returns the non-contiguous substring matches of the word, compared to the wordToMatchAgainst.
	/// </summary>
	/// <param name="word"></param>
	/// <param name="wordToMatchAgainst"></param>
	/// <returns></returns>
	public static PaletteFilterMatch[]? MatchesSubString(string word, string wordToMatchAgainst)
	{
		return MatchesSubStringRecurse(word.ToLower(), wordToMatchAgainst.ToLower(), 0, 0);
	}

	private static PaletteFilterMatch[]? MatchesSubStringRecurse(string word, string wordToMatchAgainst, int wordStart, int wordMatchStart)
	{
		if (wordStart == word.Length)
		{
			return Array.Empty<PaletteFilterMatch>();
		}
		if (wordMatchStart == wordToMatchAgainst.Length)
		{
			return null;
		}

		if (word[wordStart] == wordToMatchAgainst[wordMatchStart])
		{
			PaletteFilterMatch[]? result = MatchesSubStringRecurse(word, wordToMatchAgainst, wordStart + 1, wordMatchStart + 1);
			if (result is not null)
			{
				return Join(new PaletteFilterMatch(wordMatchStart, wordMatchStart + 1), result);
			}
			return null;
		}

		return MatchesSubStringRecurse(word, wordToMatchAgainst, wordStart, wordMatchStart + 1);
	}
}
