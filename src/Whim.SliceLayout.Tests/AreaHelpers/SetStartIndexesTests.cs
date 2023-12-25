using FluentAssertions;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SetStartIndexesTests
{
	public static IEnumerable<object[]> SetStartIndexes_Data()
	{
		// Empty
		yield return new object[]
		{
			new ParentArea(isRow: true),
			new ParentArea(isRow: true, (1.0, new OverflowArea(isRow: true))),
			1,
		};

		// Primary stack
		yield return new object[]
		{
			SliceLayouts.CreatePrimaryStackArea(),
			new ParentArea(
				isRow: true,
				(0.5, new SliceArea(order: 0, maxChildren: 1) { StartIndex = 0 }),
				(0.5, new OverflowArea() { StartIndex = 1 })
			),
			2,
		};

		// Multi-column 2-1-0
		yield return new object[]
		{
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			new ParentArea(
				isRow: true,
				(1.0 / 3.0, new SliceArea(order: 0, maxChildren: 2) { StartIndex = 0 }),
				(1.0 / 3.0, new SliceArea(order: 1, maxChildren: 1) { StartIndex = 2 }),
				(1.0 / 3.0, new OverflowArea() { StartIndex = 3 })
			),
			3,
		};

		// Secondary primary
		yield return new object[]
		{
			SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
			new ParentArea(
				isRow: true,
				(0.25, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 1 }),
				(0.5, new SliceArea(order: 0, maxChildren: 1) { StartIndex = 0 }),
				(0.25, new OverflowArea() { StartIndex = 3 })
			),
			3,
		};

		// Overflow column
		yield return new object[]
		{
			SampleSliceLayouts.CreateOverflowColumnLayout(),
			new ParentArea(isRow: false, (1.0, new OverflowArea() { StartIndex = 0 })),
			1,
		};

		// Nested
		yield return new object[]
		{
			SampleSliceLayouts.CreateNestedLayout(),
			new ParentArea(
				isRow: true,
				(0.5, new SliceArea(order: 0, maxChildren: 2) { StartIndex = 0 }),
				(
					0.5,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 2 }),
						(0.5, new OverflowArea() { StartIndex = 4 })
					)
				)
			),
			3,
		};

		// Create OverflowArea
		yield return new object[]
		{
			new ParentArea(isRow: false, (1.0, new SliceArea(maxChildren: 4))),
			new ParentArea(isRow: false, (1.0, new OverflowArea())),
			1
		};

		// Create OverflowArea for nested
		yield return new object[]
		{
			new ParentArea(
				isRow: true,
				(0.5, new SliceArea(order: 2, maxChildren: 4)),
				(
					0.5,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 0, maxChildren: 4)),
						(0.5, new SliceArea(order: 1, maxChildren: 4))
					)
				)
			),
			new ParentArea(
				isRow: true,
				(0.5, new OverflowArea()),
				(
					0.5,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 0, maxChildren: 4)),
						(0.5, new SliceArea(order: 1, maxChildren: 4))
					)
				)
			),
			3
		};

		// Create OverflowArea for complex nested
		yield return new object[]
		{
			new ParentArea(
				isRow: true,
				(0.25, new SliceArea(order: 3, maxChildren: 4)),
				(
					0.25,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 0, maxChildren: 4)),
						(0.5, new SliceArea(order: 1, maxChildren: 4))
					)
				),
				(
					0.25,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 4, maxChildren: 4)),
						(0.5, new SliceArea(order: 5, maxChildren: 4))
					)
				),
				(0.25, new SliceArea(order: 2, maxChildren: 4))
			),
			new ParentArea(
				isRow: true,
				(0.25, new SliceArea(order: 3, maxChildren: 4)),
				(
					0.25,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 0, maxChildren: 4)),
						(0.5, new SliceArea(order: 1, maxChildren: 4))
					)
				),
				(
					0.25,
					new ParentArea(
						isRow: false,
						(0.5, new SliceArea(order: 4, maxChildren: 4)),
						(0.5, new OverflowArea())
					)
				),
				(0.25, new SliceArea(order: 2, maxChildren: 4))
			),
			6
		};
	}

	[Theory]
	[MemberData(nameof(SetStartIndexes_Data))]
	public void SetStartIndexes(ParentArea parentArea, ParentArea expectedParentArea, int sliceAreasCount)
	{
		// When the slice areas are set up
		(ParentArea resultingParentArea, BaseSliceArea[] sliceAreas) = parentArea.SetStartIndexes();

		// Then the start indexes are set correctly.
		resultingParentArea.Should().BeEquivalentTo(expectedParentArea);

		// Ensure order of slice areas is correct.
		Assert.Equal(sliceAreasCount, sliceAreas.Length);
		sliceAreas.Should().BeInAscendingOrder(a => a.StartIndex);
	}
}
