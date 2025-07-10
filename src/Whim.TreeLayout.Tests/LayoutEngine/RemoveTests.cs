using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class RemoveTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsNull(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given
		TreeLayoutEngine engine = new(ctx, plugin, identity);

		// When
		ILayoutEngine result = engine.RemoveWindow(window);

		// Then
		Assert.Same(engine, result);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsWindow_Success(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		// When
		ILayoutEngine result = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window));
		Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsWindow_WrongWindow(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window,
		IWindow wrongWindow
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		// When
		ILayoutEngine result = engine.RemoveWindow(wrongWindow);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsSplitNode_CannotFindWindow(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IWindow wrongWindow
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		// When
		ILayoutEngine result = engine.RemoveWindow(wrongWindow);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsSplitNode_ReplaceRoot(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window1).AddWindow(window2);

		// When
		ILayoutEngine result = engine.RemoveWindow(window1);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsSplitNode_ReplaceChildSplitNodeWithWindow(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.MoveWindowToPoint(window3, new Point<double>() { X = 0.75, Y = 0.75 });

		// When
		ILayoutEngine result = engine.RemoveWindow(window3);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.False(result.ContainsWindow(window3));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsSplitNode_ReplaceRootWithSplitNode(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.MoveWindowToPoint(window3, new Point<double>() { X = 0.75, Y = 0.75 });

		// When
		ILayoutEngine result = engine.RemoveWindow(window1);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_RootIsSplitNode_KeepRoot(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.AddWindow(window3);

		// When
		ILayoutEngine result = engine.RemoveWindow(window2);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.False(result.ContainsWindow(window2));
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_WindowWasMinimized(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow minimizedWindow,
		IWindow window1,
		IWindow window2
	)
	{
		// Given a window has been minimized
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.MinimizeWindowStart(minimizedWindow)
			.AddWindow(window1)
			.AddWindow(window2);

		// When the window is removed
		ILayoutEngine result = engine.RemoveWindow(minimizedWindow);

		// Then it is no longer tracked
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(minimizedWindow));
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Remove_WindowWasVisible(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow windowWasVisible,
		IWindow minimizedWindow1,
		IWindow minimizedWindow2
	)
	{
		// Given only one window is visible, and the others are minimized
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.MinimizeWindowStart(minimizedWindow1)
			.MinimizeWindowStart(minimizedWindow2)
			.AddWindow(windowWasVisible);

		// When the visible window is removed
		ILayoutEngine result = engine.RemoveWindow(windowWasVisible);

		// Then it is no longer tracked
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(windowWasVisible));
		Assert.True(result.ContainsWindow(minimizedWindow1));
		Assert.True(result.ContainsWindow(minimizedWindow2));
		Assert.Equal(2, result.Count);
	}
}
