using FluentAssertions;
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
		new SliceLayoutEngine(
			ctx,
			plugin,
			identity,
			new ParentArea(isRow: false, (1, new SliceArea(order: 0, maxChildren: 0)))
		)
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
		ILayoutEngine sut = SliceLayouts.CreateRowLayout(ctx, plugin, identity);

		// Then
		// Then
		new SliceLayoutEngine(
			ctx,
			plugin,
			identity,
			new ParentArea(isRow: true, (1, new SliceArea(order: 0, maxChildren: 0)))
		)
		{
			Name = "Row",
		}
			.Should()
			.BeEquivalentTo(sut);
	}

	[Theory, AutoSubstituteData]
	public void CreatePrimaryStackLayout(IContext ctx, ISliceLayoutPlugin plugin)
	{
		// Given
		LayoutEngineIdentity identity = new();
		ILayoutEngine sut = SliceLayouts.CreatePrimaryStackLayout(ctx, plugin, identity);

		// Then
		new SliceLayoutEngine(
			ctx,
			plugin,
			identity,
			new ParentArea(isRow: true, (0.5, new SliceArea(order: 0, maxChildren: 0)), (0.5, new OverflowArea()))
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
}
