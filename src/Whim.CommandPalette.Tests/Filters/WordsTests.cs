using Xunit;

namespace Whim.CommandPalette.Tests;

public class WordsTests
{
	[InlineData("alpha", "alpha", new int[] { 0, 5 })]
	[InlineData("alpha", "alphasomething", new int[] { 0, 5 })]
	[InlineData("a", "alpha", new int[] { 0, 1 })]
	[InlineData("A", "alpha", new int[] { 0, 1 })]
	[InlineData("AlPh", "alPHA", new int[] { 0, 4 })]
	[InlineData("gp", "Git: Pull", new int[] { 0, 1 }, new int[] { 5, 6 })]
	[InlineData("g p", "Git: Pull", new int[] { 0, 1 }, new int[] { 5, 6 })]
	[InlineData("gipu", "Git: Pull", new int[] { 0, 2 }, new int[] { 5, 7 })]
	[InlineData("gp", "Category: Git: Pull", new int[] { 10, 11 }, new int[] { 15, 16 })]
	[InlineData("g p", "Category: Git: Pull", new int[] { 10, 11 }, new int[] { 15, 16 })]
	[InlineData("gipu", "Category: Git: Pull", new int[] { 10, 12 }, new int[] { 15, 17 })]
	[InlineData("git: プル", "git: プル", new int[] { 0, 7 })]
	[InlineData("git プル", "git: プル", new int[] { 0, 3 }, new int[] { 5, 7 })]
	[InlineData("öäk", "Öhm: Älles Klar", new int[] { 0, 1 }, new int[] { 5, 6 }, new int[] { 11, 12 })]
	[InlineData("C++", "C/C++: command", new int[] { 2, 5 })]
	[InlineData(".", ":")]
	[InlineData(".", ".", new int[] { 0, 1 })]
	[InlineData("bar", "foo-bar", new int[] { 4, 7 })]
	[InlineData("bar test", "foo-bar test", new int[] { 4, 12 })]
	[InlineData("fbt", "foo-bar test", new int[] { 0, 1 }, new int[] { 4, 5 }, new int[] { 8, 9 })]
	[InlineData("bar test", "foo-bar (test)", new int[] { 4, 8 }, new int[] { 9, 13 })]
	[InlineData("foo bar", "foo (bar)", new int[] { 0, 4 }, new int[] { 5, 8 })]
	[InlineData("foo bar", "foo-bar", new int[] { 0, 3 }, new int[] { 4, 7 })]
	[InlineData("foo bar", "123 foo-bar 456", new int[] { 4, 7 }, new int[] { 8, 11 })]
	[InlineData("foo-bar", "foo bar", new int[] { 0, 3 }, new int[] { 4, 7 })]
	[InlineData("foo:bar", "foo:bar", new int[] { 0, 7 })]
	[Theory]
	public void MatchesWordsSeparate_Ok(string query, string target, params int[][] expected)
	{
		PaletteFilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesWordsSeparate, query, target, expectedMatches);
	}

	[InlineData("alpha", "alp")]
	[InlineData("x", "alpha")]
	[InlineData("it", "Git: Pull")]
	[InlineData("ll", "Git: Pull")]
	[InlineData("bar est", "foo-bar test")]
	[InlineData("fo ar", "foo-bar test")]
	[InlineData("for", "foo-bar test")]
	[Theory]
	public void MatchesWordsSeparate_NotOk(string query, string target)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesWordsSeparate, query, target);
	}

	[InlineData("pu", "Category: Git: Pull", new int[] { 15, 17 })]
	[Theory]
	public void MatchesWordsContiguous_Ok(string query, string target, params int[][] expected)
	{
		PaletteFilterTextMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(PaletteFilters.MatchesWordsContiguous, query, target, expectedMatches);
	}

	[InlineData("gipu", "Category: Git: Pull")]
	[Theory]
	public void MatchesWordsContiguous_NotOk(string query, string target)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesWordsContiguous, query, target);
	}
}
