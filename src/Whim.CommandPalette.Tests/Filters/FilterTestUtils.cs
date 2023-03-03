using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whim.CommandPalette.Tests;

public static class FilterTestUtils
{
	public static void FilterOk(
		PaletteFilter filter,
		string word,
		string wordToMatchAgainst,
		FilterTextMatch[]? expectedMatches = null
	)
	{
		FilterTextMatch[]? actualMatches = filter(word, wordToMatchAgainst);

		if (expectedMatches == null)
		{
			Assert.IsNull(actualMatches);
			return;
		}

		Assert.IsNotNull(actualMatches);
		Assert.AreEqual(expectedMatches.Length, actualMatches!.Length);
		for (int i = 0; i < expectedMatches.Length; i++)
		{
			FilterTextMatch expected = expectedMatches[i];
			FilterTextMatch actual = actualMatches[i];

			Assert.AreEqual(expected.Start, actual.Start);
			Assert.AreEqual(expected.End, actual.End);
		}
	}

	public static void FilterNotOk(PaletteFilter filter, string word, string wordToMatchAgainst)
	{
		FilterOk(filter, word, wordToMatchAgainst, null);
	}

	public static FilterTextMatch[] CreateExpectedMatches(params int[][] expected)
	{
		FilterTextMatch[] result = new FilterTextMatch[expected.Length];
		for (int i = 0; i < expected.Length; i++)
		{
			int[] pair = expected[i];
			result[i] = new FilterTextMatch(pair[0], pair[1]);
		}
		return result;
	}
}
