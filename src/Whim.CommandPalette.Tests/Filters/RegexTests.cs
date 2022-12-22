using Xunit;

namespace Whim.CommandPalette.Tests;

public class RegexTests
{
	[Fact]
	public void MatchesRegex()
	{
		PaletteFilterTextMatch[]? matches = PaletteFilters.MatchesRegex("regex", "regex");
		Assert.NotNull(matches);
		Assert.Equal(0, matches[0].Start);
		Assert.Equal(5, matches[0].End);
	}

	[Fact]
	public void MatchesRegex_Fail()
	{
		PaletteFilterTextMatch[]? matches = PaletteFilters.MatchesRegex("regex", "reg");
		Assert.Null(matches);
	}
}
