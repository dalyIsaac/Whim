using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whim.CommandPalette.Tests;

[TestClass]
public class WordsTests
{
	[TestMethod]
	[DataRow("alpha", "alpha", new int[] { 0, 5 })]
	[DataRow("alpha", "alphasomething", new int[] { 0, 5 })]
	[DataRow("a", "alpha", new int[] { 0, 1 })]
	[DataRow("A", "alpha", new int[] { 0, 1 })]
	[DataRow("AlPh", "alPHA", new int[] { 0, 4 })]
	[DataRow("gp", "Git: Pull", new int[] { 0, 1 }, new int[] { 5, 6 })]
	[DataRow("g p", "Git: Pull", new int[] { 0, 1 }, new int[] { 5, 6 })]
	[DataRow("gipu", "Git: Pull", new int[] { 0, 2 }, new int[] { 5, 7 })]
	[DataRow("gp", "Category: Git: Pull", new int[] { 10, 11 }, new int[] { 15, 16 })]
	[DataRow("g p", "Category: Git: Pull", new int[] { 10, 11 }, new int[] { 15, 16 })]
	[DataRow("gipu", "Category: Git: Pull", new int[] { 10, 12 }, new int[] { 15, 17 })]
	[DataRow("git: プル", "git: プル", new int[] { 0, 7 })]
	[DataRow("git プル", "git: プル", new int[] { 0, 3 }, new int[] { 5, 7 })]
	[DataRow("öäk", "Öhm: Älles Klar", new int[] { 0, 1 }, new int[] { 5, 6 }, new int[] { 11, 12 })]
	[DataRow("C++", "C/C++: command", new int[] { 2, 5 })]
	[DataRow(".", ":")]
	[DataRow(".", ".", new int[] { 0, 1 })]
	[DataRow("bar", "foo-bar", new int[] { 4, 7 })]
	[DataRow("bar test", "foo-bar test", new int[] { 4, 12 })]
	[DataRow("fbt", "foo-bar test", new int[] { 0, 1 }, new int[] { 4, 5 }, new int[] { 8, 9 })]
	[DataRow("bar test", "foo-bar (test)", new int[] { 4, 8 }, new int[] { 9, 13 })]
	[DataRow("foo bar", "foo (bar)", new int[] { 0, 4 }, new int[] { 5, 8 })]
	[DataRow("foo bar", "foo-bar", new int[] { 0, 3 }, new int[] { 4, 7 })]
	[DataRow("foo bar", "123 foo-bar 456", new int[] { 4, 7 }, new int[] { 8, 11 })]
	[DataRow("foo-bar", "foo bar", new int[] { 0, 3 }, new int[] { 4, 7 })]
	[DataRow("foo:bar", "foo:bar", new int[] { 0, 7 })]
	public void MatchesWordsSeparate_Ok(string query, string target, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesWordsSeparate, query, target, expectedMatches);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow("alpha", "alp")]
	[DataRow("x", "alpha")]
	[DataRow("it", "Git: Pull")]
	[DataRow("ll", "Git: Pull")]
	[DataRow("bar est", "foo-bar test")]
	[DataRow("fo ar", "foo-bar test")]
	[DataRow("for", "foo-bar test")]
	public void MatchesWordsSeparate_NotOk(string query, string target)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesWordsSeparate, query, target);
	}

	[TestMethod]
	[DataRow("pu", "Category: Git: Pull", new int[] { 15, 17 })]
	public void MatchesWordsContiguous_Ok(string query, string target, params int[][] expected)
	{
		FilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesWordsContiguous, query, target, expectedMatches);
	}

	[TestMethod]
	[DataRow("gipu", "Category: Git: Pull")]
	public void MatchesWordsContiguous_NotOk(string query, string target)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesWordsContiguous, query, target);
	}
}
