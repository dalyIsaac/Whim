using Xunit;

namespace Whim.CommandPalette.Tests;

public class CamelCaseTests
{
	public static TheoryData<string, string, FilterTextMatch[]?> MatchesCamelCase_Ok_Data =>
		new()
		{
			{ "", "anything", Array.Empty<FilterTextMatch>() },
			{ "alpha", "alpha", new FilterTextMatch[] { new(0, 5) } },
			{ "AlPhA", "alpha", new FilterTextMatch[] { new(0, 5) } },
			{ "alpha", "alphasomething", new FilterTextMatch[] { new(0, 5) } },
			{ "c", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1) } },
			{ "cc", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1), new(5, 6) } },
			{ "ccr", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1), new(5, 6), new(9, 10) } },
			{ "cacr", "CamelCaseRocks", new FilterTextMatch[] { new(0, 2), new(5, 6), new(9, 10) } },
			{ "cacar", "CamelCaseRocks", new FilterTextMatch[] { new(0, 2), new(5, 7), new(9, 10) } },
			{ "ccarocks", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1), new(5, 7), new(9, 14) } },
			{ "cr", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1), new(9, 10) } },
			{ "fba", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 5) } },
			{ "fbar", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 6) } },
			{ "fbara", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 7) } },
			{ "fbaa", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 5), new(6, 7) } },
			{ "fbaab", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 5), new(6, 8) } },
			{ "c2d", "canvasCreation2D", new FilterTextMatch[] { new(0, 1), new(14, 16) } },
			{ "cce", "_canvasCreationEvent", new FilterTextMatch[] { new(1, 2), new(7, 8), new(15, 16) } },
			{ "Debug Console", "Open: Debug Console", new FilterTextMatch[] { new(6, 19) } },
			{ "Debug console", "Open: Debug Console", new FilterTextMatch[] { new(6, 19) } },
			{ "debug console", "Open: Debug Console", new FilterTextMatch[] { new(6, 19) } },
			{ "long", "Very long string which is over 60 characters long which will fail the test", null },
			{ "a", "A B C D E F", null },
			{ "a", "a BCDEF", null },
		};

	[Theory]
	[MemberData(nameof(MatchesCamelCase_Ok_Data))]
	public void MatchesCamelCase_Ok(string word, string wordToMatchAgainst, FilterTextMatch[]? expected)
	{
		FilterTestUtils.FilterOk(PaletteFilters.MatchesCamelCase, word, wordToMatchAgainst, expected);
	}

	[InlineData("", "")]
	[InlineData("alpha", "alph")]
	[Theory]
	public void MatchesCamelCase_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesCamelCase, word, wordToMatchAgainst);
	}

	[InlineData("alpha", true)]
	[InlineData("Alpha", true)]
	[InlineData("Alpha Beta", true)]
	[InlineData("Alpha Beta Gamma Delta Zeta", true)]
	[InlineData("A B C D E F", false)]
	[InlineData("ALPHABETAGAMMADELTAEPSILONZETA", true)]
	[InlineData("alphabetagammadeltaepsilonzeta", true)]
	[InlineData("AlphaBetaGammaDeltaEpsilonZetaEta", false)]
	[Theory]
	public void IsCamelCasePattern(string word, bool expected)
	{
		Assert.Equal(expected, PaletteFilters.IsCamelCasePattern(word));
	}

	[InlineData(0.8f, 0, true)]
	[InlineData(0.8f, 1, false)]
	[InlineData(0, 1, false)]
	[InlineData(0, 0, false)]
	[Theory]
	public void IsUpperCaseWord(float upperPercent, int upperCount, bool expected)
	{
		Assert.Equal(expected, PaletteFilters.IsUpperCaseWord(new CamelCaseAnalysis(upperPercent, upperCount, 0, 0)));
	}

	[InlineData(0, 1, 1, 0, true)]
	[InlineData(0.8f, 0.2, 1, 0, false)]
	[InlineData(0, 1, 0, 0, false)]
	[InlineData(0, 0, 0, 0, false)]
	[InlineData(0.7f, 0.3f, 0.5f, 0.2f, false)]
	[Theory]
	public void IsCamelCaseWord(float upper, float lower, float alpha, float numeric, bool expected)
	{
		Assert.Equal(expected, PaletteFilters.IsCamelCaseWord(new CamelCaseAnalysis(upper, lower, alpha, numeric)));
	}

	[InlineData("a", 0, 1)]
	[InlineData("alpha", 0, 5)]
	[InlineData("alphaBeta", 0, 5)]
	[InlineData("alpha0", 0, 5)]
	[InlineData("alpha beta", 0, 6)]
	[Theory]
	public void FindNextCamelCaseAnchor(string camelCaseWord, int startIndex, int nextAnchor)
	{
		Assert.Equal(nextAnchor, PaletteFilters.FindNextCamelCaseAnchor(camelCaseWord, startIndex));
	}
}
