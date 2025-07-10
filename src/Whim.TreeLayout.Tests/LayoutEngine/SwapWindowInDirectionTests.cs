using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class SwapWindowInDirectionTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void SwapWindowInDirection_RootIsNull(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given
		TreeLayoutEngine engine = new(ctx, plugin, identity);

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window);

		// Then
		Assert.Same(engine, result);
		Assert.False(result.ContainsWindow(window));
		Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void SwapWindowInDirection_NoAdjacentNodeInDirection(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window1);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.Same(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window1,
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 50,
					Height = 100,
				},
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = window2,
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 0,
					Width = 50,
					Height = 100,
				},
				WindowSize = WindowSize.Normal,
			},
		}
			.Should()
			.Equal(windowStates);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void SwapWindowInDirection_SameParent(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Right, window1);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window2,
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 50,
					Height = 100,
				},
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = window1,
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 0,
					Width = 50,
					Height = 100,
				},
				WindowSize = WindowSize.Normal,
			},
		}
			.Should()
			.Equal(windowStates);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void SwapWindowInDirection_DifferentParents(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.MoveWindowToPoint(window3, new Point<double>() { X = 0.75, Y = 0.75 });

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Right, window1);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window2,
				Rectangle = new Rectangle<int>() { Width = 50, Height = 100 },
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = window1,
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 0,
					Width = 50,
					Height = 50,
				},
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = window3,
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50,
				},
				WindowSize = WindowSize.Normal,
			},
		}
			.Should()
			.Equal(windowStates);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void SwapWindowInDirection_Diagonal(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow topLeft,
		IWindow topRight,
		IWindow bottomLeft,
		IWindow bottomRight,
		IMonitor monitor
	)
	{
		// Given

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.AddWindow(topLeft)
			.AddWindow(topRight)
			.MoveWindowToPoint(bottomLeft, new Point<double>() { X = 0.25, Y = 0.9 })
			.MoveWindowToPoint(bottomRight, new Point<double>() { X = 0.75, Y = 0.9 });

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.LeftUp, bottomRight);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = bottomRight,
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 50,
					Height = 50,
				},
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = bottomLeft,
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 50,
					Width = 50,
					Height = 50,
				},
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = topRight,
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 0,
					Width = 50,
					Height = 50,
				},
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Window = topLeft,
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50,
				},
				WindowSize = WindowSize.Normal,
			},
		}
			.Should()
			.Equal(windowStates);
	}
}
