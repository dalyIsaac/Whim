using Xunit;

namespace Whim.CommandPalette.Tests;

public static class FilterTestUtils
{
	public static void FilterOk(PaletteFilter filter, string word, string wordToMatchAgainst, PaletteFilterMatch[]? expectedMatches = null)
	{
		PaletteFilterMatch[]? actualMatches = filter(word, wordToMatchAgainst);

		if (expectedMatches == null)
		{
			Assert.Null(actualMatches);
			return;
		}

		Assert.NotNull(actualMatches);
		Assert.Equal(expectedMatches.Length, actualMatches!.Length);
		for (int i = 0; i < expectedMatches.Length; i++)
		{
			PaletteFilterMatch expected = expectedMatches[i];
			PaletteFilterMatch actual = actualMatches[i];

			Assert.Equal(expected.Start, actual.Start);
			Assert.Equal(expected.End, actual.End);
		}
	}

	public static void FilterNotOk(PaletteFilter filter, string word, string wordToMatchAgainst)
	{
		FilterOk(filter, word, wordToMatchAgainst, null);
	}

	public static PaletteFilterMatch[] CreateExpectedMatches(params int[][] expected)
	{
		PaletteFilterMatch[] result = new PaletteFilterMatch[expected.Length];
		for (int i = 0; i < expected.Length; i++)
		{
			int[] pair = expected[i];
			result[i] = new PaletteFilterMatch(pair[0], pair[1]);
		}
		return result;
	}
}
