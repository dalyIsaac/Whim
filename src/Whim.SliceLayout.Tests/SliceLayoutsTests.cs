using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

// NOTE: We deliberately use FluentAssertions to deeply compare the objects, instead of comparing
// the properties one by one, or comparing by reference.
public class SliceLayoutsTests
{
	[Theory, AutoSubstituteData]
	public void CreateColumnLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		SliceLayoutEngine sut = (SliceLayoutEngine)SliceLayouts.CreateColumnLayout(ctx, plugin, identity);

		// Then
		new SliceLayoutEngine(ctx, plugin, identity, new ParentArea(isRow: false, (1, new OverflowArea(isRow: false))))
		{
			Name = "Column",
		}
			.Should()
			.BeEquivalentTo(sut);
	}

	[Theory, AutoSubstituteData]
	public void CreateRowLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		SliceLayoutEngine sut = (SliceLayoutEngine)SliceLayouts.CreateRowLayout(ctx, plugin, identity);

		// Then
		new SliceLayoutEngine(ctx, plugin, identity, new ParentArea(isRow: true, (1, new OverflowArea(isRow: true))))
		{
			Name = "Row",
		}
			.Should()
			.BeEquivalentTo(sut);
	}

	[Theory, AutoSubstituteData]
	public void SanityCheck_RowNotEqualToColumn(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// A sanity check to ensure that the two layouts are not equal.
		// Given
		LayoutEngineIdentity identity = new();
		SliceLayoutEngine row = (SliceLayoutEngine)SliceLayouts.CreateRowLayout(ctx, plugin, identity);
		SliceLayoutEngine column = (SliceLayoutEngine)SliceLayouts.CreateColumnLayout(ctx, plugin, identity);

		// Then
		row.Should().NotBeEquivalentTo(column);
	}

	[Theory, AutoSubstituteData]
	public void SanityCheck_RowsNotEqual(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		SliceLayoutEngine row1 = new(
			ctx,
			plugin,
			identity,
			new ParentArea(isRow: true, (1, new OverflowArea(isRow: true)))
		);
		SliceLayoutEngine row2 = new(ctx, plugin, identity, new ParentArea(isRow: true, (1, new SliceArea(0, 0))));

		// Then
		row1.Should().NotBeEquivalentTo(row2);
	}

	[Theory, AutoSubstituteData]
	public void CreatePrimaryStackLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		SliceLayoutEngine sut = (SliceLayoutEngine)SliceLayouts.CreatePrimaryStackLayout(ctx, plugin, identity);

		// Then
		new SliceLayoutEngine(
			ctx,
			plugin,
			identity,
			new ParentArea(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 1)), (0.5, new OverflowArea()))
		)
		{
			Name = "Primary stack",
		}
			.Should()
			.BeEquivalentTo(sut);
	}

	[Fact]
	public void CreateMultiColumnArea_MultipleOverflows()
	{
		// Given
		ParentArea sut = SliceLayouts.CreateMultiColumnArea([2, 1, 0, 0]);

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

	[Theory]
	[InlineAutoSubstituteData(1, 2)]
	[InlineAutoSubstituteData(3, 1)]
	public void CreateSecondaryPrimaryLayout(
		uint primaryCount,
		uint secondaryCount,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		LayoutEngineIdentity identity = new();
		SliceLayoutEngine sut = (SliceLayoutEngine)
			SliceLayouts.CreateSecondaryPrimaryLayout(ctx, plugin, identity, primaryCount, secondaryCount);

		// Then
		new SliceLayoutEngine(
			ctx,
			plugin,
			identity,
			new ParentArea(
				isRow: true,
				(0.25, new SliceArea(order: 1, maxChildren: secondaryCount)),
				(0.5, new SliceArea(order: 0, maxChildren: primaryCount)),
				(0.25, new OverflowArea())
			)
		)
		{
			Name = "Secondary primary",
		}
			.Should()
			.BeEquivalentTo(sut);
	}
}
