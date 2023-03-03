using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class MatcherResultTests
{
	[TestMethod]
	public void FormattedTitle()
	{
		// Given
		string text = "normal highlighted normal";
		FilterTextMatch[] matches = new[] { new FilterTextMatch(7, 18), };
		Mock<IVariantRowModel<int>> modelMock = new();
		modelMock.Setup(m => m.Title).Returns(text);

		// When
		MatcherResult<int> matcherResult = new(modelMock.Object, matches, 0);

		// Then
		Assert.AreEqual(3, matcherResult.FormattedTitle.Segments.Count);
		Assert.AreEqual("normal ", matcherResult.FormattedTitle.Segments[0].Text);
		Assert.IsFalse(matcherResult.FormattedTitle.Segments[0].IsHighlighted);
		Assert.AreEqual("highlighted", matcherResult.FormattedTitle.Segments[1].Text);
		Assert.IsTrue(matcherResult.FormattedTitle.Segments[1].IsHighlighted);
		Assert.AreEqual(" normal", matcherResult.FormattedTitle.Segments[2].Text);
		Assert.IsFalse(matcherResult.FormattedTitle.Segments[2].IsHighlighted);
	}
}
