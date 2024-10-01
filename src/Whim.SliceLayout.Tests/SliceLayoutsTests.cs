using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SliceLayoutsTests
{
	[Theory, AutoSubstituteData]
	public void CreateColumnLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		ILayoutEngine sut = SliceLayouts.CreateColumnLayout(ctx, plugin, identity);

		// Then
		Assert.Equal(
			new SliceLayoutEngine(
				ctx,
				plugin,
				identity,
				new ParentArea(isRow: false, (1, new OverflowArea(isRow: false)))
			)
			{
				Name = "Column",
			},
			sut
		);
	}

	[Theory, AutoSubstituteData]
	public void CreateRowLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		ILayoutEngine sut = SliceLayouts.CreateRowLayout(ctx, plugin, identity);

		// Then
		Assert.Equal(
			new SliceLayoutEngine(
				ctx,
				plugin,
				identity,
				new ParentArea(isRow: true, (1, new OverflowArea(isRow: true)))
			)
			{
				Name = "Row",
			},
			sut
		);
	}

	[Theory, AutoSubstituteData]
	public void RowNotEqualToColumn(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// A sanity check to ensure that the two layouts are not equal.
		// Given
		LayoutEngineIdentity identity = new();
		ILayoutEngine row = SliceLayouts.CreateRowLayout(ctx, plugin, identity);
		ILayoutEngine column = SliceLayouts.CreateColumnLayout(ctx, plugin, identity);

		// Then
		Assert.NotEqual(row, column);
	}

	[Theory, AutoSubstituteData]
	public void CreatePrimaryStackLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		ILayoutEngine sut = SliceLayouts.CreatePrimaryStackLayout(ctx, plugin, identity);

		// Then
		Assert.Equal(
			new SliceLayoutEngine(
				ctx,
				plugin,
				identity,
				new ParentArea(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 1)), (0.5, new OverflowArea()))
			)
			{
				Name = "Primary stack",
			},
			sut
		);
	}

	[Fact]
	public void CreateMultiColumnArea_MultipleOverflows()
	{
		// Given
		ParentArea sut = SliceLayouts.CreateMultiColumnArea([2, 1, 0, 0]);

		// Then
		Assert.Equal(
			new ParentArea(
				isRow: true,
				(0.25, new SliceArea(order: 0, maxChildren: 2)),
				(0.25, new SliceArea(order: 1, maxChildren: 1)),
				(0.25, new SliceArea(order: 2, maxChildren: 0)),
				(0.25, new OverflowArea())
			),
			sut
		);
	}

	// TODO: SecondaryPrimaryArea
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
		ILayoutEngine sut = SliceLayouts.CreateSecondaryPrimaryLayout(
			ctx,
			plugin,
			identity,
			primaryCount,
			secondaryCount
		);

		// Then
		Assert.Equal(
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
			},
			sut
		);
	}
}
