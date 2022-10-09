using Xunit;

namespace Whim.CommandPalette.Tests;

public class UtilsTests
{
	private static PaletteFilter NewFilter(int[] counters, int i, bool r)
	{
		return (string word, string wordToMatchAgainst) =>
		{
			counters[i]++;
			return r ? new[] { new PaletteFilterTextMatch(0, word.Length) } : null;
		};
	}

	[InlineData(0, true, 1, false, new int[] { 1, 0 })]
	[InlineData(0, true, 1, true, new int[] { 1, 0 })]
	[InlineData(0, false, 1, true, new int[] { 1, 1 })]
	[Theory]
	public void Or(int i1, bool r1, int i2, bool r2, int[] expected)
	{
		int[] counters = new int[2];
		PaletteFilter filter = PaletteFilters.Or(NewFilter(counters, i1, r1), NewFilter(counters, i2, r2));
		FilterTestUtils.FilterOk(filter, "anything", "anything", new PaletteFilterTextMatch[] { new PaletteFilterTextMatch(0, 8) });
		Assert.Equal(expected, counters);
	}

	[InlineData(0, false, 1, false, new int[] { 1, 1 })]
	[Theory]
	public void Or_NotOk(int i1, bool r1, int i2, bool r2, int[] expected)
	{
		int[] counters = new int[2];
		PaletteFilter filter = PaletteFilters.Or(NewFilter(counters, i1, r1), NewFilter(counters, i2, r2));
		FilterTestUtils.FilterNotOk(filter, "anything", "anything");
		Assert.Equal(expected, counters);
	}
}
