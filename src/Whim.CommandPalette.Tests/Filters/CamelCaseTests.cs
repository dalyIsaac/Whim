using Xunit;

namespace Whim.CommandPalette.Tests;

public class CamelCaseTests
{
	[InlineData("", "anything")]
	[InlineData("alpha", "alpha", new int[] { 0, 5 })]
	[InlineData("AlPhA", "alpha", new int[] { 0, 5 })]
	[InlineData("alpha", "alphasomething", new int[] { 0, 5 })]
	[InlineData("c", "CamelCaseRocks", new int[] { 0, 1 })]
	[InlineData("cc", "CamelCaseRocks", new int[] { 0, 1 }, new int[] { 5, 6 })]
	[InlineData("ccr", "CamelCaseRocks", new int[] { 0, 1 }, new int[] { 5, 6 }, new int[] { 9, 10 })]
	[InlineData("cacr", "CamelCaseRocks", new int[] { 0, 2 }, new int[] { 5, 6 }, new int[] { 9, 10 })]
	[InlineData("cacar", "CamelCaseRocks", new int[] { 0, 2 }, new int[] { 5, 7 }, new int[] { 9, 10 })]
	[InlineData("ccarocks", "CamelCaseRocks", new int[] { 0, 1 }, new int[] { 5, 7 }, new int[] { 9, 14 })]
	[InlineData("cr", "CamelCaseRocks", new int[] { 0, 1 }, new int[] { 9, 10 })]
	[InlineData("fba", "FooBarAbe", new int[] { 0, 1 }, new int[] { 3, 5 })]
	[InlineData("fbar", "FooBarAbe", new int[] { 0, 1 }, new int[] { 3, 6 })]
	[InlineData("fbara", "FooBarAbe", new int[] { 0, 1 }, new int[] { 3, 7 })]
	[InlineData("fbaa", "FooBarAbe", new int[] { 0, 1 }, new int[] { 3, 5 }, new int[] { 6, 7 })]
	[InlineData("fbaab", "FooBarAbe", new int[] { 0, 1 }, new int[] { 3, 5 }, new int[] { 6, 8 })]
	[InlineData("c2d", "canvasCreation2D", new int[] { 0, 1 }, new int[] { 14, 16 })]
	[InlineData("cce", "_canvasCreationEvent", new int[] { 1, 2 }, new int[] { 7, 8 }, new int[] { 15, 16 })]
	[InlineData("Debug Console", "Open: Debug Console", new int[] { 6, 19 })]
	[InlineData("Debug console", "Open: Debug Console", new int[] { 6, 19 })]
	[InlineData("debug console", "Open: Debug Console", new int[] { 6, 19 })]
	[Theory]
	public void MatchesCamelCase_Ok(string word, string wordToMatchAgainst, params int[][] expected)
	{
		PaletteFilterMatch[] expectedMatches = FilterTestUtils.CreateExpectedMatches(expected);
		FilterTestUtils.FilterOk(Filters.MatchesCamelCase, word, wordToMatchAgainst, expectedMatches);
	}

	[InlineData("", "")]
	[InlineData("alpha", "alph")]
	[Theory]
	public void MatchesCamelCase_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(Filters.MatchesCamelCase, word, wordToMatchAgainst);
	}
}
