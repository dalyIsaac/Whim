using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class PrefixTests
{
	[TestMethod]
	[DataRow("", "anything")]
	[DataRow("alpha", "alpha", new int[] { 0, 5 })]
	[DataRow("alpha", "alphasomething", new int[] { 0, 5 })]
	[DataRow("a", "alpha", new int[] { 0, 1 })]
	public void MatchesStrictPrefix_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesStrictPrefix, word, wordToMatchAgainst, expectedMatches);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow("alpha", "alp")]
	[DataRow("x", "alpha")]
	[DataRow("A", "alpha")]
	[DataRow("AlPh", "alPHA")]
	public void MatchesStrictPrefix_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesStrictPrefix, word, wordToMatchAgainst);
	}

	[TestMethod]
	[DataRow("alpha", "alpha", new int[] { 0, 5 })]
	[DataRow("alpha", "alphasomething", new int[] { 0, 5 })]
	[DataRow("a", "alpha", new int[] { 0, 1 })]
	[DataRow("ä", "Älpha", new int[] { 0, 1 })]
	[DataRow("A", "alpha", new int[] { 0, 1 })]
	[DataRow("AlPh", "alPHA", new int[] { 0, 4 })]
	public void MatchesPrefix_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesPrefix, word, wordToMatchAgainst, expectedMatches);
	}

	[TestMethod]
	[DataRow("alpha", "alp")]
	[DataRow("x", "alpha")]
	[DataRow("T", "4")]
	public void MatchesPrefix_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesPrefix, word, wordToMatchAgainst);
	}
}
