using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MinimizeWindowTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MinimizeWindowStart(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given an empty layout engine
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		// When we minimize a window
		ILayoutEngine result = engine.MinimizeWindowStart(window);

		// Then the window is added to the layout engine
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MinimizeWindowStart_AlreadyMinimized(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given the window is already minimized
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).MinimizeWindowStart(window);

		// When we minimize the window again
		ILayoutEngine result = engine.MinimizeWindowStart(window);

		// Then nothing changes
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MinimizeWindowStart_WindowAlreadyAdded(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given the window is already added
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		// When we minimize the window
		ILayoutEngine result = engine.MinimizeWindowStart(window);

		// Then we get a new layout engine
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MinimizeWindowEnd(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given the window is minimized
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).MinimizeWindowStart(window);

		// When we unminimize the window
		ILayoutEngine result = engine.MinimizeWindowEnd(window);

		// Then the window is removed from the layout engine
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MinimizeWindowEnd_NotMinimized(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given the window is not minimized
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		// When we unminimize the window
		ILayoutEngine result = engine.MinimizeWindowEnd(window);

		// Then nothing changes
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void MinimizeWindowEnd_WindowAlreadyAdded(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given the window is already added
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		// When we unminimize the window
		ILayoutEngine result = engine.MinimizeWindowEnd(window);

		// Then we get the same layout engine
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}
}
