using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class UtilsTests
{
	private static PaletteFilter NewFilter(int[] counters, int i, bool r)
	{
		return (string word, string wordToMatchAgainst) =>
		{
			counters[i]++;
			return r ? new[] { new FilterTextMatch(0, word.Length) } : null;
		};
	}

	[TestMethod]
	[DataRow(0, true, 1, false, new int[] { 1, 0 })]
	[DataRow(0, true, 1, true, new int[] { 1, 0 })]
	[DataRow(0, false, 1, true, new int[] { 1, 1 })]
	public void Or(int i1, bool r1, int i2, bool r2, int[] expected)
	{
		int[] counters = new int[2];
		PaletteFilter filter = PaletteFilters.Or(NewFilter(counters, i1, r1), NewFilter(counters, i2, r2));
		FilterTestUtils.FilterOk(filter, "anything", "anything", new FilterTextMatch[] { new FilterTextMatch(0, 8) });
		CollectionAssert.AreEqual(expected, counters);
	}

	[TestMethod]
	[DataRow(0, false, 1, false, new int[] { 1, 1 })]
	public void Or_NotOk(int i1, bool r1, int i2, bool r2, int[] expected)
	{
		int[] counters = new int[2];
		PaletteFilter filter = PaletteFilters.Or(NewFilter(counters, i1, r1), NewFilter(counters, i2, r2));
		FilterTestUtils.FilterNotOk(filter, "anything", "anything");
		CollectionAssert.AreEqual(expected, counters);
	}
}
