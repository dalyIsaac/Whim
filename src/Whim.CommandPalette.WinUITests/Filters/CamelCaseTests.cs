using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class CamelCaseTests
{
	public static IEnumerable<object[]> MatchesCamelCase_Ok_Data()
	{
		yield return new object[] { "", "anything", Array.Empty<FilterTextMatch>() };
		yield return new object[] { "alpha", "alpha", new FilterTextMatch[] { new(0, 5) } };
		yield return new object[] { "AlPhA", "alpha", new FilterTextMatch[] { new(0, 5) } };
		yield return new object[] { "alpha", "alphasomething", new FilterTextMatch[] { new(0, 5) } };
		yield return new object[] { "c", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1) } };
		yield return new object[] { "cc", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1), new(5, 6) } };
		yield return new object[]
		{
			"ccr",
			"CamelCaseRocks",
			new FilterTextMatch[] { new(0, 1), new(5, 6), new(9, 10) }
		};
		yield return new object[]
		{
			"cacr",
			"CamelCaseRocks",
			new FilterTextMatch[] { new(0, 2), new(5, 6), new(9, 10) }
		};
		yield return new object[]
		{
			"cacar",
			"CamelCaseRocks",
			new FilterTextMatch[] { new(0, 2), new(5, 7), new(9, 10) }
		};
		yield return new object[]
		{
			"ccarocks",
			"CamelCaseRocks",
			new FilterTextMatch[] { new(0, 1), new(5, 7), new(9, 14) }
		};
		yield return new object[] { "cr", "CamelCaseRocks", new FilterTextMatch[] { new(0, 1), new(9, 10) } };
		yield return new object[] { "fba", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 5) } };
		yield return new object[] { "fbar", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 6) } };
		yield return new object[] { "fbara", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 7) } };
		yield return new object[] { "fbaa", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 5), new(6, 7) } };
		yield return new object[] { "fbaab", "FooBarAbe", new FilterTextMatch[] { new(0, 1), new(3, 5), new(6, 8) } };
		yield return new object[] { "c2d", "canvasCreation2D", new FilterTextMatch[] { new(0, 1), new(14, 16) } };
		yield return new object[]
		{
			"cce",
			"_canvasCreationEvent",
			new FilterTextMatch[] { new(1, 2), new(7, 8), new(15, 16) }
		};
		yield return new object[] { "Debug Console", "Open: Debug Console", new FilterTextMatch[] { new(6, 19) } };
		yield return new object[] { "Debug console", "Open: Debug Console", new FilterTextMatch[] { new(6, 19) } };
		yield return new object[] { "debug console", "Open: Debug Console", new FilterTextMatch[] { new(6, 19) } };
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

	[DataTestMethod]
	[DynamicData(nameof(MatchesCamelCase_Ok_Data), DynamicDataSourceType.Method)]
	public void MatchesCamelCase_Ok(string word, string wordToMatchAgainst, FilterTextMatch[]? expectedMatches)
	{
		FilterTestUtils.FilterOk(PaletteFilters.MatchesCamelCase, word, wordToMatchAgainst, expectedMatches);
	}

	[DataTestMethod]
	[DataRow("", "")]
	[DataRow("alpha", "alph")]
	public void MatchesCamelCase_NotOk(string word, string wordToMatchAgainst)
	{
		FilterTestUtils.FilterNotOk(PaletteFilters.MatchesCamelCase, word, wordToMatchAgainst);
	}

	[DataTestMethod]
	[DataRow("alpha", true)]
	[DataRow("Alpha", true)]
	[DataRow("Alpha Beta", true)]
	[DataRow("Alpha Beta Gamma Delta Zeta", true)]
	[DataRow("A B C D E F", false)]
	[DataRow("ALPHABETAGAMMADELTAEPSILONZETA", true)]
	[DataRow("alphabetagammadeltaepsilonzeta", true)]
	[DataRow("AlphaBetaGammaDeltaEpsilonZetaEta", false)]
	public void IsCamelCasePattern(string word, bool expected)
	{
		Assert.AreEqual(expected, PaletteFilters.IsCamelCasePattern(word));
	}

	[DataTestMethod]
	[DataRow(0.8f, 0, true)]
	[DataRow(0.8f, 1, false)]
	[DataRow(0, 1, false)]
	[DataRow(0, 0, false)]
	public void IsUpperCaseWord(float upperPercent, int upperCount, bool expected)
	{
		Assert.AreEqual(
			expected,
			PaletteFilters.IsUpperCaseWord(new CamelCaseAnalysis(upperPercent, upperCount, 0, 0))
		);
	}

	[DataTestMethod]
	[DataRow(0, 1, 1, 0, true)]
	[DataRow(0.8f, 0.2f, 1, 0, false)]
	[DataRow(0, 1, 0, 0, false)]
	[DataRow(0, 0, 0, 0, false)]
	[DataRow(0.7f, 0.3f, 0.5f, 0.2f, false)]
	public void IsCamelCaseWord(float upper, float lower, float alpha, float numeric, bool expected)
	{
		Assert.AreEqual(expected, PaletteFilters.IsCamelCaseWord(new CamelCaseAnalysis(upper, lower, alpha, numeric)));
	}

	[DataTestMethod]
	[DataRow("a", 0, 1)]
	[DataRow("alpha", 0, 5)]
	[DataRow("alphaBeta", 0, 5)]
	[DataRow("alpha0", 0, 5)]
	[DataRow("alpha beta", 0, 6)]
	public void FindNextCamelCaseAnchor(string camelCaseWord, int startIndex, int nextAnchor)
	{
		Assert.AreEqual(nextAnchor, PaletteFilters.FindNextCamelCaseAnchor(camelCaseWord, startIndex));
	}
}
