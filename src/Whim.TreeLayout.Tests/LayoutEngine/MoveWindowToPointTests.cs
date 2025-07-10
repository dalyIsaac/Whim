using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MoveWindowToPointTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsNull(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given
		TreeLayoutEngine engine = new(ctx, plugin, identity);
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window, point);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsWindowNode_Right(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				[
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
				]
			);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsWindowNode_Down(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.7 };
		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				[
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 50,
						},
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50,
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsWindowNode_Left(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1);

		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.5 };
		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				[
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
				]
			);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsWindowNode_Up(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.3 };
		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				[
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 50,
						},
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50,
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsSplitNode_DoesNotContainPoint(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IPoint<double> point = new Point<double>() { X = 1.7, Y = 0.5 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3, point);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.False(result.ContainsWindow(window3));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsSplitNode_AddInDirection(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				[
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100,
						},
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100,
						},
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100,
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MoveWindowToPoint_RootIsSplitNode_AddInDifferentDirection(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IPoint<double> point = new Point<double>() { X = 0.75, Y = 0.8 };

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				[
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
				]
			);
	}

	[InlineAutoSubstituteData<TreeCustomization>(0.25, 0.25)]
	[InlineAutoSubstituteData<TreeCustomization>(0.25, 0.75)]
	[InlineAutoSubstituteData<TreeCustomization>(0.75, 0.25)]
	[InlineAutoSubstituteData<TreeCustomization>(0.75, 0.75)]
	[Theory]
	internal void MoveWindowToPoint_AlreadyContainsWindow(
		double x,
		double y,
		IWindow window,
		IMonitor monitor,
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window);
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		IPoint<double> point = new Point<double>() { X = x, Y = y };

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window, point);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Single(windowStates);

		windowStates
			.Should()
			.Equal(
				[
					new WindowState()
					{
						Window = window,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 100,
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);
	}
}
