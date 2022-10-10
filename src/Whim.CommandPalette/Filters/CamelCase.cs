using System;

namespace Whim.CommandPalette;

internal record struct CamelCaseAnalysis(float Upper, float Lower, float Alpha, float Numeric);

public static partial class PaletteFilters
{
	/// <summary>
	/// Returns the matches for the word, where the word is a camel case substring of the wordToMatchAgainst.
	/// </summary>
	public static PaletteFilterTextMatch[]? MatchesCamelCase(string word, string wordToMatchAgainst)
	{
		if (string.IsNullOrEmpty(wordToMatchAgainst))
		{
			return null;
		}

		word = word.Trim();

		if (word.Length == 0)
		{
			return Array.Empty<PaletteFilterTextMatch>();
		}

		if (!IsCamelCasePattern(word) || wordToMatchAgainst.Length > 60)
		{
			return null;
		}

		CamelCaseAnalysis analysis = AnalyzeCamelCaseWord(wordToMatchAgainst);
		if (!IsCamelCaseWord(analysis))
		{
			if (!IsUpperCaseWord(analysis))
			{
				return null;
			}

			wordToMatchAgainst = wordToMatchAgainst.ToLower();
		}

		PaletteFilterTextMatch[]? result = null;
		int i = 0;

		word = word.ToLower();
		while (i < wordToMatchAgainst.Length && (result = MatchesCamelCaseRecurse(word, wordToMatchAgainst, 0, i)) == null)
		{
			i = FindNextCamelCaseAnchor(wordToMatchAgainst, i + 1);
		}

		return result;
	}

	private static PaletteFilterTextMatch[]? MatchesCamelCaseRecurse(string word, string wordToMatchAgainst, int wordStart, int wordMatchStart)
	{
		if (wordStart == word.Length)
		{
			return Array.Empty<PaletteFilterTextMatch>();
		}
		else if (wordMatchStart == wordToMatchAgainst.Length)
		{
			return null;
		}
		else if (word[wordStart] != char.ToLower(wordToMatchAgainst[wordMatchStart]))
		{
			return null;
		}
		else
		{
			PaletteFilterTextMatch[]? result = MatchesCamelCaseRecurse(word, wordToMatchAgainst, wordStart + 1, wordMatchStart + 1);
			int nextUpperIndex = wordMatchStart + 1;
			while (result == null && (nextUpperIndex = FindNextCamelCaseAnchor(wordToMatchAgainst, nextUpperIndex)) < wordToMatchAgainst.Length)
			{
				result = MatchesCamelCaseRecurse(word, wordToMatchAgainst, wordStart + 1, nextUpperIndex);
				nextUpperIndex++;
			}
			return result == null ? null : Join(new PaletteFilterTextMatch(wordMatchStart, wordMatchStart + 1), result);
		}
	}

	private static CamelCaseAnalysis AnalyzeCamelCaseWord(string word)
	{
		int upper = 0, lower = 0, alpha = 0, numeric = 0;

		for (int i = 0; i < word.Length; i++)
		{
			char code = word[i];

			if (char.IsUpper(code)) { upper++; }
			if (char.IsLower(code)) { lower++; }
			if (char.IsLetterOrDigit(code)) { alpha++; }
			if (char.IsDigit(code)) { numeric++; }
		}

		float upperPercent = (float)upper / word.Length;
		float lowerPercent = (float)lower / word.Length;
		float alphaPercent = (float)alpha / word.Length;
		float numericPercent = (float)numeric / word.Length;

		return new CamelCaseAnalysis(upperPercent, lowerPercent, alphaPercent, numericPercent);
	}

	private static bool IsUpperCaseWord(CamelCaseAnalysis analysis)
	{
		return analysis.Lower == 0.0f && analysis.Upper > 0.6f;
	}

	private static bool IsCamelCaseWord(CamelCaseAnalysis analysis)
	{
		return analysis.Lower > 0.2f && analysis.Upper < 0.8f && analysis.Alpha > 0.6f && analysis.Numeric < 0.2f;
	}

	/// <summary>
	/// Heuristic to avoid computing camel case matcher for words that don't look like camel case patterns.
	/// </summary>
	/// <param name="word"></param>
	/// <returns></returns>
	private static bool IsCamelCasePattern(string word)
	{
		int upper = 0;
		int lower = 0;
		int whitespace = 0;

		foreach (char ch in word)
		{
			if (char.IsUpper(ch))
			{
				upper++;
			}
			else if (char.IsLower(ch))
			{
				lower++;
			}

			else if (char.IsWhiteSpace(ch))
			{
				whitespace++;
			}
		}

		if ((upper == 0 || lower == 0) && whitespace == 0)
		{
			return word.Length <= 30;
		}

		return upper <= 5;
	}

	/// <summary>
	/// Finds the next camel-case anchor. For example, in "ride_camelEgypt123", the anchors are at
	/// 4, 10, 15.
	/// </summary>
	/// <param name="camelCaseWord"></param>
	/// <param name="startIndex"></param>
	/// <returns></returns>
	private static int FindNextCamelCaseAnchor(string camelCaseWord, int startIndex)
	{
		for (int i = startIndex; i < camelCaseWord.Length; i++)
		{
			char ch = camelCaseWord[i];
			if (char.IsUpper(ch) || char.IsDigit(ch) || (i > 0 && !char.IsLetterOrDigit(camelCaseWord[i - 1])))
			{
				return i;
			}
		}

		return camelCaseWord.Length;
	}
}
