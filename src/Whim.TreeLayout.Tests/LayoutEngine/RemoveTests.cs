using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class RemoveTests
{
	[Theory, AutoSubstituteData]
	public void Remove_RootIsNull(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		ILayoutEngine result = engine.RemoveWindow(window);

		// Then
		Assert.Same(engine, result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_RootIsWindow_Success(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		// When
		ILayoutEngine result = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window));
		Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void Remove_RootIsWindow_WrongWindow(IWindow window, IWindow wrongWindow)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		// When
		ILayoutEngine result = engine.RemoveWindow(wrongWindow);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void Remove_RootIsSplitNode_CannotFindWindow(IWindow window1, IWindow window2, IWindow wrongWindow)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		// When
		ILayoutEngine result = engine.RemoveWindow(wrongWindow);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void Remove_RootIsSplitNode_ReplaceRoot(IWindow window1, IWindow window2)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		// When
		ILayoutEngine result = engine.RemoveWindow(window1);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void Remove_RootIsSplitNode_ReplaceChildSplitNodeWithWindow(
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
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

	[Theory, AutoSubstituteData]
	public void Remove_RootIsSplitNode_ReplaceRootWithSplitNode(IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
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

	[Theory, AutoSubstituteData]
	public void Remove_RootIsSplitNode_KeepRoot(IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
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

	[Theory, AutoSubstituteData]
	public void Remove_WindowWasMinimized(IWindow minimizedWindow, IWindow window1, IWindow window2)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.MinimizeWindowStart(minimizedWindow)
			.AddWindow(window1)
			.AddWindow(window2);

		// When
		ILayoutEngine result = engine.RemoveWindow(minimizedWindow);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(minimizedWindow));
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);
	}
}
