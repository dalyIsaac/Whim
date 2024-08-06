using FluentAssertions;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class PruneTests
{
	public static TheoryData<ParentArea, int, ParentArea> Prune_PrimaryStack =>
		new()
		{
			// Empty
			{ SliceLayouts.CreatePrimaryStackArea(), 0, new ParentArea(isRow: true) },
			// Fill primary
			{
				SliceLayouts.CreatePrimaryStackArea(),
				1,
				new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 1)))
			},
			// Fill overflow
			{
				SliceLayouts.CreatePrimaryStackArea(),
				3,
				new ParentArea(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 1)), (0.5, new OverflowArea()))
			}
		};

	public static TheoryData<ParentArea, int, ParentArea> Prune_MultiColumn =>
		new()
		{
			// Empty
			{ SliceLayouts.CreateMultiColumnArea([2, 1, 0]), 0, new ParentArea(isRow: true) },
			// Single window
			{
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				1,
				new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2)))
			},
			// Fill primary
			{
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				2,
				new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2)))
			},
			// Fill secondary
			{
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				3,
				new ParentArea(
					isRow: true,
					(0.5, new SliceArea(order: 0, maxChildren: 2)),
					(0.5, new SliceArea(order: 1, maxChildren: 1) { StartIndex = 2 })
				)
			},
			// Fill overflow
			{
				SliceLayouts.CreateMultiColumnArea([2, 1, 0]),
				4,
				new ParentArea(
					isRow: true,
					(1.0 / 3.0, new SliceArea(order: 0, maxChildren: 2)),
					(1.0 / 3.0, new SliceArea(order: 1, maxChildren: 1) { StartIndex = 2 }),
					(1.0 / 3.0, new OverflowArea() { StartIndex = 3 })
				)
			}
		};

	public static TheoryData<ParentArea, int, ParentArea> Prune_SecondaryPrimary =>
		new()
		{
			// Empty
			{ SliceLayouts.CreateSecondaryPrimaryArea(1, 2), 0, new ParentArea(isRow: true) },
			// Fill primary
			{
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				1,
				new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 1)))
			},
			// Fill secondary
			{
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				3,
				new ParentArea(
					isRow: true,
					(0.25 + 0.125, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 1 }),
					(0.5 + 0.125, new SliceArea(order: 0, maxChildren: 1))
				)
			},
			// Fill overflow
			{
				SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
				5,
				new ParentArea(
					isRow: true,
					(0.25, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 1 }),
					(0.5, new SliceArea(order: 0, maxChildren: 1)),
					(0.25, new OverflowArea() { StartIndex = 2 })
				)
			}
		};

	public static TheoryData<ParentArea, int, ParentArea> Prune_OverflowColumn =>
		new()
		{
			// Empty
			{ SampleSliceLayouts.CreateOverflowColumnLayout(), 0, new ParentArea(isRow: false) },
			// Single window
			{
				SampleSliceLayouts.CreateOverflowColumnLayout(),
				1,
				new ParentArea(isRow: false, (1.0, new OverflowArea()))
			},
			// Fill overflow
			{
				SampleSliceLayouts.CreateOverflowColumnLayout(),
				2,
				new ParentArea(isRow: false, (1.0, new OverflowArea()))
			}
		};

	public static TheoryData<ParentArea, int, ParentArea> Prune_Nested =>
		new()
		{
			// Empty
			{ SampleSliceLayouts.CreateNestedLayout(), 0, new ParentArea(isRow: true) },
			// Single window
			{
				SampleSliceLayouts.CreateNestedLayout(),
				1,
				new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2)))
			},
			// Fill primary
			{
				SampleSliceLayouts.CreateNestedLayout(),
				2,
				new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2)))
			},
			// Fill secondary
			{
				SampleSliceLayouts.CreateNestedLayout(),
				4,
				new ParentArea(
					isRow: true,
					(0.5, new SliceArea(order: 0, maxChildren: 2)),
					(0.5, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 2 })
				)
			},
			// Fill overflow
			{
				SampleSliceLayouts.CreateNestedLayout(),
				6,
				new ParentArea(
					isRow: true,
					(0.5, new SliceArea(order: 0, maxChildren: 2)),
					(
						0.5,
						new ParentArea(
							isRow: false,
							(0.5, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 2 }),
							(0.5, new OverflowArea() { StartIndex = 4 })
						)
					)
				)
			}
		};

	// Take the first overflow area
	public static TheoryData<ParentArea, int, ParentArea> Prune_MultipleOverflows =>
		new()
		{
			{
				new ParentArea(
					isRow: true,
					(0.5, new SliceArea(order: 0, maxChildren: 1)),
					(0.25, new OverflowArea()),
					(0.25, new OverflowArea())
				),
				1,
				new ParentArea(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 1)))
			},
			{
				new ParentArea(
					isRow: true,
					(0.5, new SliceArea(order: 0, maxChildren: 1)),
					(0.25, new OverflowArea()),
					(0.25, new OverflowArea())
				),
				2,
				new ParentArea(
					isRow: true,
					(0.5, new SliceArea(order: 0, maxChildren: 1)),
					(0.5, new OverflowArea() { StartIndex = 1 })
				)
			}
		};

	[Theory]
	[MemberData(nameof(Prune_PrimaryStack))]
	[MemberData(nameof(Prune_MultiColumn))]
	[MemberData(nameof(Prune_SecondaryPrimary))]
	[MemberData(nameof(Prune_OverflowColumn))]
	[MemberData(nameof(Prune_Nested))]
	public void Prune(ParentArea area, int windowCount, ParentArea expected)
	{
		area.SetStartIndexes();
		ParentArea pruned = area.Prune(windowCount);
		expected.Should().BeEquivalentTo(pruned);
	}
}
