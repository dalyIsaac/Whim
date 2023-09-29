using NSubstitute;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatcherResultTests
{
	[Fact]
	public void FormattedTitle()
	{
		// Given
		string text = "normal highlighted normal";
		FilterTextMatch[] matches = new[] { new FilterTextMatch(7, 18), };
		IVariantRowModel<int> modelMock = Substitute.For<IVariantRowModel<int>>();
		modelMock.Title.Returns(text);

		// When
		MatcherResult<int> matcherResult = new(modelMock, matches, 0);

		// Then
		Assert.Equal(3, matcherResult.FormattedTitle.Segments.Count);
		Assert.Equal("normal ", matcherResult.FormattedTitle.Segments[0].Text);
		Assert.False(matcherResult.FormattedTitle.Segments[0].IsHighlighted);
		Assert.Equal("highlighted", matcherResult.FormattedTitle.Segments[1].Text);
		Assert.True(matcherResult.FormattedTitle.Segments[1].IsHighlighted);
		Assert.Equal(" normal", matcherResult.FormattedTitle.Segments[2].Text);
		Assert.False(matcherResult.FormattedTitle.Segments[2].IsHighlighted);
	}
}
