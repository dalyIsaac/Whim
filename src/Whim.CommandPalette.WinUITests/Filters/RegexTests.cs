using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class RegexTests
{
	[TestMethod]
	[DataRow("regex", "regex", new int[] { 0, 5 })]
	public void MatchesRegex_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesRegex, word, wordToMatchAgainst, expectedMatches);
	}

	[TestMethod]
	[DataRow("regex", "reg")]
	public void MatchesRegex_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesRegex, word, wordToMatchAgainst);
	}
}
