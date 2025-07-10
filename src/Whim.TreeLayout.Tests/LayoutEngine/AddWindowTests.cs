using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class AddWindowTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	public void AddWindow_RootIsNull(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given
		TreeLayoutEngine engine = new(ctx, plugin, identity);

		// When
		ILayoutEngine result = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	public void AddWindow_RootIsWindow(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IMonitor monitor
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1);
		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window2);
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
	internal void AddWindow_RootIsSplitNode_LastFocusedWindowIsNull(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor,
		LayoutEngineIdentity identity
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, null);
		TreeCustomization.SetAddWindowDirection(plugin, Direction.Right);

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
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
						Window = window2,
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
						Window = window3,
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
	internal void AddWindow_RootIsSplitNode_CannotFindLastFocusedWindow(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		MutableRootSector root,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window3);

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
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
						Window = window2,
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
						Window = window3,
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
	internal void AddWindow_RootIsSplitNode_LastFocusedWindowIsLeft(
		IContext ctx,
		MutableRootSector root,
		Workspace workspace,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, window1);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
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
	internal void AddWindow_RootIsSplitNode_AddInDifferentDirection(
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
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);
		TreeCustomization.SetAddWindowDirection(plugin, Direction.Down);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.NotSame(engine, result);
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

	[Theory, AutoSubstituteData<TreeCustomization>]
	public void AddWindow_WindowAlreadyPresent(
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
		ILayoutEngine result = engine.AddWindow(window2);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then
		Assert.Same(engine, result);
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
	public void AddWindow_WindowWasMinimized(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IMonitor monitor,
		IWindow window
	)
	{
		// Given a window has been added
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When the window has been minimized
		ILayoutEngine result = engine.MinimizeWindowStart(window).AddWindow(window);
		IWindowState[] windowStates = [.. result.DoLayout(rect, monitor)];

		// Then the window is minimized, and a new layout engine is created
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);

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
