using Xunit;

namespace Whim.CommandPalette.Tests;

public class CamelCaseTests
{
	public static IEnumerable<object[]> MatchesCamelCase_Ok_Data()
	{
		yield return new object[] { "", "anything", Array.Empty<PaletteFilterTextMatch>() };
		yield return new object[] { "alpha", "alpha", new PaletteFilterTextMatch[] { new(0, 5) } };
		yield return new object[] { "AlPhA", "alpha", new PaletteFilterTextMatch[] { new(0, 5) } };
		yield return new object[] { "alpha", "alphasomething", new PaletteFilterTextMatch[] { new(0, 5) } };
		yield return new object[] { "c", "CamelCaseRocks", new PaletteFilterTextMatch[] { new(0, 1) } };
		yield return new object[] { "cc", "CamelCaseRocks", new PaletteFilterTextMatch[] { new(0, 1), new(5, 6) } };
		yield return new object[]
		{
			"ccr",
			"CamelCaseRocks",
			new PaletteFilterTextMatch[] { new(0, 1), new(5, 6), new(9, 10) }
		};
		yield return new object[]
		{
			"cacr",
			"CamelCaseRocks",
			new PaletteFilterTextMatch[] { new(0, 2), new(5, 6), new(9, 10) }
		};
		yield return new object[]
		{
			"cacar",
			"CamelCaseRocks",
			new PaletteFilterTextMatch[] { new(0, 2), new(5, 7), new(9, 10) }
		};
		yield return new object[]
		{
			"ccarocks",
			"CamelCaseRocks",
			new PaletteFilterTextMatch[] { new(0, 1), new(5, 7), new(9, 14) }
		};
		yield return new object[] { "cr", "CamelCaseRocks", new PaletteFilterTextMatch[] { new(0, 1), new(9, 10) } };
		yield return new object[] { "fba", "FooBarAbe", new PaletteFilterTextMatch[] { new(0, 1), new(3, 5) } };
		yield return new object[] { "fbar", "FooBarAbe", new PaletteFilterTextMatch[] { new(0, 1), new(3, 6) } };
		yield return new object[] { "fbara", "FooBarAbe", new PaletteFilterTextMatch[] { new(0, 1), new(3, 7) } };
		yield return new object[]
		{
			"fbaa",
			"FooBarAbe",
			new PaletteFilterTextMatch[] { new(0, 1), new(3, 5), new(6, 7) }
		};
		yield return new object[]
		{
			"fbaab",
			"FooBarAbe",
			new PaletteFilterTextMatch[] { new(0, 1), new(3, 5), new(6, 8) }
		};
		yield return new object[]
		{
			"c2d",
			"canvasCreation2D",
			new PaletteFilterTextMatch[] { new(0, 1), new(14, 16) }
		};
		yield return new object[]
		{
			"cce",
			"_canvasCreationEvent",
			new PaletteFilterTextMatch[] { new(1, 2), new(7, 8), new(15, 16) }
		};
		yield return new object[]
		{
			"Debug Console",
			"Open: Debug Console",
			new PaletteFilterTextMatch[] { new(6, 19) }
		};
		yield return new object[]
		{
			"Debug console",
			"Open: Debug Console",
			new PaletteFilterTextMatch[] { new(6, 19) }
		};
		yield return new object[]
		{
			"debug console",
			"Open: Debug Console",
			new PaletteFilterTextMatch[] { new(6, 19) }
		};
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		yield return new object[]
		{
			"long",
			"Very long string which is over 60 characters long which will fail the test",
			null
		};
		yield return new object[] { "a", "A B C D E F", null };
		yield return new object[] { "a", "a BCDEF", null };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Theory]
	[MemberData(nameof(MatchesCamelCase_Ok_Data))]
	public void MatchesCamelCase_Ok(string word, string wordToMatchAgainst, PaletteFilterTextMatch[]? expected)
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
