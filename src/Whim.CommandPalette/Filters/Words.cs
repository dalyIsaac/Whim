using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

public static partial class PaletteFilters
{
	/// <summary>
	/// Matches beginning of words supporting non-ASCII languages.
	/// Matches substrings of the word with beginnings of the words in the target. E.g., "gp" or "g p" will match "Git: Pull".
	/// Useful in cases where the target is words (e.g., command labels).
	/// </summary>
	public static PaletteFilterTextMatch[]? MatchesWordsContiguous(string word, string target)
	{
		return MatchesWordsBase(word, target, true);
	}

	/// <summary>
	/// Matches beginning of words supporting non-ASCII languages.
	/// Matches words with the beginnings of the words in the target. E.g., "pul" will match "Git: Pull".
	/// </summary>
	public static PaletteFilterTextMatch[]? MatchesWordsSeparate(string word, string target)
	{
		return MatchesWordsBase(word, target, false);
	}

	private static PaletteFilterTextMatch[]? MatchesWordsBase(string word, string target, bool contiguous = false)
	{
		if (string.IsNullOrEmpty(target))
		{
			return null;
		}

		PaletteFilterTextMatch[]? result = null;
		int i = 0;

		word = word.ToLower();
		target = target.ToLower();
		while (i < target.Length && (result = MatchesWordsRecurse(word, target, 0, i, contiguous)) == null)
		{
			i = NextWord(target, i + 1);
		}

		return result;
	}

	private static PaletteFilterTextMatch[]? MatchesWordsRecurse(
		string word,
		string wordToMatchAgainst,
		int i,
		int j,
		bool contiguous
	)
	{
		if (i == word.Length)
		{
			return Array.Empty<PaletteFilterTextMatch>();
		}
		else if (j == wordToMatchAgainst.Length)
		{
			return null;
		}
		else if (!CharactersMatch(word[i], wordToMatchAgainst[j]))
		{
			return null;
		}

		int nextWordIndex = j + 1;
		PaletteFilterTextMatch[]? result = MatchesWordsRecurse(word, wordToMatchAgainst, i + 1, j + 1, contiguous);
		if (!contiguous)
		{
			while (
				result == null
				&& (nextWordIndex = NextWord(wordToMatchAgainst, nextWordIndex)) < wordToMatchAgainst.Length
			)
			{
				result = MatchesWordsRecurse(word, wordToMatchAgainst, i + 1, nextWordIndex, contiguous);
				nextWordIndex++;
			}
		}

		if (result == null)
		{
			return null;
		}

		// If the characters don't exactly match, then they must be word separators (see charactersMatch(...)).
		// We don't want to include this in the matches but we don't want to throw the target out all together so we return `result`.
		if (word[i] != wordToMatchAgainst[j])
		{
			return result;
		}

		return PaletteFilters.Join(new PaletteFilterTextMatch(j, j + 1), result);
	}

	private static int NextWord(string word, int start)
	{
		for (int i = start; i < word.Length; i++)
		{
			if (IsWordSeparator(word[i]) || (i > 0 && IsWordSeparator(word[i - 1])))
			{
				return i;
			}
		}
		return word.Length;
	}

	private static readonly HashSet<char> wordSeparators = new("()[]{}<>`'\"-/;:,.?!".ToCharArray());

	private static bool IsWordSeparator(char code)
	{
		return char.IsWhiteSpace(code) || wordSeparators.Contains(code);
	}

	private static bool CharactersMatch(char codeA, char codeB)
	{
		return (codeA == codeB) || (IsWordSeparator(codeA) && IsWordSeparator(codeB));
	}
}
