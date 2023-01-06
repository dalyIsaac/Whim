using Xunit;

namespace Whim.CommandPalette.Tests;

public class PrefixTests
{
	[InlineData("", "anything")]
	[InlineData("alpha", "alpha", new int[] { 0, 5 })]
	[InlineData("alpha", "alphasomething", new int[] { 0, 5 })]
	[InlineData("a", "alpha", new int[] { 0, 1 })]
	[Theory]
	public void MatchesStrictPrefix_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesStrictPrefix, word, wordToMatchAgainst, expectedMatches);
	}

	[InlineData("", "")]
	[InlineData("alpha", "alp")]
	[InlineData("x", "alpha")]
	[InlineData("A", "alpha")]
	[InlineData("AlPh", "alPHA")]
	[Theory]
	public void MatchesStrictPrefix_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesStrictPrefix, word, wordToMatchAgainst);
	}

	[InlineData("alpha", "alpha", new int[] { 0, 5 })]
	[InlineData("alpha", "alphasomething", new int[] { 0, 5 })]
	[InlineData("a", "alpha", new int[] { 0, 1 })]
	[InlineData("ä", "Älpha", new int[] { 0, 1 })]
	[InlineData("A", "alpha", new int[] { 0, 1 })]
	[InlineData("AlPh", "alPHA", new int[] { 0, 4 })]
	[Theory]
	public void MatchesPrefix_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesPrefix, word, wordToMatchAgainst, expectedMatches);
	}

	[InlineData("alpha", "alp")]
	[InlineData("x", "alpha")]
	[InlineData("T", "4")]
	[Theory]
	public void MatchesPrefix_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesPrefix, word, wordToMatchAgainst);
	}
}
