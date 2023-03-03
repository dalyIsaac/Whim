using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whim.CommandPalette.Tests;

[TestClass]
public class SubstringTests
{
	[TestMethod]
	[DataRow("cela", "cancelAnimationFrame()", new int[] { 3, 7 })]
	public void MatchesContiguousSubString_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesContiguousSubString, word, wordToMatchAgainst, expectedMatches);
	}

	[TestMethod]
	public void MatchesContiguousSubString_NotFound()
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesContiguousSubString, "cmm", "cancelAnimationFrame()");
	}

	[TestMethod]
	[DataRow("cmm", "cancelAnimationFrame()", new int[] { 0, 1 }, new int[] { 9, 10 }, new int[] { 18, 19 })]
	[DataRow("abc", "abcabc", new int[] { 0, 3 })]
	[DataRow("abc", "aaabbbccc", new int[] { 0, 1 }, new int[] { 3, 4 }, new int[] { 6, 7 })]
	public void MatchesSubString_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesSubString, word, wordToMatchAgainst, expectedMatches);
	}

	[TestMethod]
	[DataRow("aaaaaaaaaaaaaaaaaaaax", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
	public void MatchesSubString_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesSubString, word, wordToMatchAgainst);
	}
}
