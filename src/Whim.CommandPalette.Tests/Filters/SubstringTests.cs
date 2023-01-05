using Xunit;

namespace Whim.CommandPalette.Tests;

public class SubstringTests
{
	[InlineData("cela", "cancelAnimationFrame()", new int[] { 3, 7 })]
	[Theory]
	public void MatchesContiguousSubString_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesContiguousSubString, word, wordToMatchAgainst, expectedMatches);
	}

	[Fact]
	public void MatchesContiguousSubString_NotFound()
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesContiguousSubString, "cmm", "cancelAnimationFrame()");
	}

	[InlineData("cmm", "cancelAnimationFrame()", new int[] { 0, 1 }, new int[] { 9, 10 }, new int[] { 18, 19 })]
	[InlineData("abc", "abcabc", new int[] { 0, 3 })]
	[InlineData("abc", "aaabbbccc", new int[] { 0, 1 }, new int[] { 3, 4 }, new int[] { 6, 7 })]
	[Theory]
	public void MatchesSubString_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesSubString, word, wordToMatchAgainst, expectedMatches);
	}

	[InlineData("aaaaaaaaaaaaaaaaaaaax", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
	[Theory]
	public void MatchesSubString_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesSubString, word, wordToMatchAgainst);
	}
}
