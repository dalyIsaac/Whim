using FluentAssertions;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SliceLayoutsTests
{
	[Fact]
	public void CreateMultiColumnArea_MultipleOverflows()
	{
		// Given
		ParentArea sut = SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0, 0 });

		// Then
		new ParentArea(
			isRow: true,
			(0.25, new SliceArea(order: 0, maxChildren: 2)),
			(0.25, new SliceArea(order: 1, maxChildren: 1)),
			(0.25, new SliceArea(order: 2, maxChildren: 0)),
			(0.25, new OverflowArea())
		)
			.Should()
			.BeEquivalentTo(sut);
	}
}
