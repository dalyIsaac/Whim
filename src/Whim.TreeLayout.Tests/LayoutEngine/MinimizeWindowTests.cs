using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MinimizeWindowTests
{
	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart(IWindow window)
	{
		// Given an empty layout engine
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When we minimize a window
		ILayoutEngine result = engine.MinimizeWindowStart(window);

		// Then the window is added to the layout engine
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_AlreadyMinimized(IWindow window)
	{
		// Given the window is already minimized
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context,
			wrapper.Plugin,
			wrapper.Identity
		).MinimizeWindowStart(window);

		// When we minimize the window again
		ILayoutEngine result = engine.MinimizeWindowStart(window);

		// Then nothing changes
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_WindowAlreadyAdded(IWindow window)
	{
		// Given the window is already added
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		// When we minimize the window
		ILayoutEngine result = engine.MinimizeWindowStart(window);

		// Then we get a new layout engine
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd(IWindow window)
	{
		// Given the window is minimized
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context,
			wrapper.Plugin,
			wrapper.Identity
		).MinimizeWindowStart(window);

		// When we unminimize the window
		ILayoutEngine result = engine.MinimizeWindowEnd(window);

		// Then the window is removed from the layout engine
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_NotMinimized(IWindow window)
	{
		// Given the window is not minimized
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When we unminimize the window
		ILayoutEngine result = engine.MinimizeWindowEnd(window);

		// Then nothing changes
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_WindowAlreadyAdded(IWindow window)
	{
		// Given the window is already added
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		// When we unminimize the window
		ILayoutEngine result = engine.MinimizeWindowEnd(window);

		// Then we get the same layout engine
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}
}
