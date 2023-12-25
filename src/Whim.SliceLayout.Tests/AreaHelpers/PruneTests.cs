using FluentAssertions;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class PruneTests
{
	public static IEnumerable<object[]> Prune_PrimaryStack()
	{
		// Empty
		yield return new object[] { SliceLayouts.CreatePrimaryStackArea(), 0, new ParentArea(isRow: true), };

		// Fill primary
		yield return new object[]
		{
			SliceLayouts.CreatePrimaryStackArea(),
			1,
			new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 1))),
		};

		// Fill overflow
		yield return new object[]
		{
			SliceLayouts.CreatePrimaryStackArea(),
			3,
			new ParentArea(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 1)), (0.5, new OverflowArea())),
		};
	}

	public static IEnumerable<object[]> Prune_MultiColumn()
	{
		// Empty
		yield return new object[]
		{
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			0,
			new ParentArea(isRow: true),
		};

		// Single window
		yield return new object[]
		{
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			1,
			new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2))),
		};

		// Fill primary
		yield return new object[]
		{
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			2,
			new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2))),
		};

		// Fill secondary
		yield return new object[]
		{
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			3,
			new ParentArea(
				isRow: true,
				(0.5, new SliceArea(order: 0, maxChildren: 2)),
				(0.5, new SliceArea(order: 1, maxChildren: 1) { StartIndex = 2 })
			),
		};

		// Fill overflow
		yield return new object[]
		{
			SliceLayouts.CreateMultiColumnArea(new uint[] { 2, 1, 0 }),
			4,
			new ParentArea(
				isRow: true,
				(1.0 / 3.0, new SliceArea(order: 0, maxChildren: 2)),
				(1.0 / 3.0, new SliceArea(order: 1, maxChildren: 1) { StartIndex = 2 }),
				(1.0 / 3.0, new OverflowArea() { StartIndex = 3 })
			),
		};
	}

	public static IEnumerable<object[]> Prune_SecondaryPrimary()
	{
		// Empty
		yield return new object[] { SliceLayouts.CreateSecondaryPrimaryArea(1, 2), 0, new ParentArea(isRow: true), };

		// Fill primary
		yield return new object[]
		{
			SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
			1,
			new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 1))),
		};

		// Fill secondary
		yield return new object[]
		{
			SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
			3,
			new ParentArea(
				isRow: true,
				(0.25 + 0.125, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 1 }),
				(0.5 + 0.125, new SliceArea(order: 0, maxChildren: 1))
			),
		};

		// Fill overflow
		yield return new object[]
		{
			SliceLayouts.CreateSecondaryPrimaryArea(1, 2),
			5,
			new ParentArea(
				isRow: true,
				(0.25, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 1 }),
				(0.5, new SliceArea(order: 0, maxChildren: 1)),
				(0.25, new OverflowArea() { StartIndex = 2 })
			),
		};
	}

	public static IEnumerable<object[]> Prune_OverflowColumn()
	{
		// Empty
		yield return new object[] { SampleSliceLayouts.CreateOverflowColumnLayout(), 0, new ParentArea(isRow: false), };

		// Single window
		yield return new object[]
		{
			SampleSliceLayouts.CreateOverflowColumnLayout(),
			1,
			new ParentArea(isRow: false, (1.0, new OverflowArea())),
		};

		// Fill overflow
		yield return new object[]
		{
			SampleSliceLayouts.CreateOverflowColumnLayout(),
			2,
			new ParentArea(isRow: false, (1.0, new OverflowArea())),
		};
	}

	public static IEnumerable<object[]> Prune_Nested()
	{
		// Empty
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 0, new ParentArea(isRow: true), };

		// Single window
		yield return new object[]
		{
			SampleSliceLayouts.CreateNestedLayout(),
			1,
			new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2))),
		};

		// Fill primary
		yield return new object[]
		{
			SampleSliceLayouts.CreateNestedLayout(),
			2,
			new ParentArea(isRow: true, (1.0, new SliceArea(order: 0, maxChildren: 2))),
		};

		// Fill secondary
		yield return new object[]
		{
			SampleSliceLayouts.CreateNestedLayout(),
			4,
			new ParentArea(
				isRow: true,
				(0.5, new SliceArea(order: 0, maxChildren: 2)),
				(0.5, new SliceArea(order: 1, maxChildren: 2) { StartIndex = 2 })
			),
		};

		// Fill overflow
		yield return new object[]
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
			),
		};
	}

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
