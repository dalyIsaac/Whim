using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class RemoveTests
{
	[Fact]
	public void Remove_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		Mock<IWindow> window = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);

		// When
		ILayoutEngine result = engine.RemoveWindow(window.Object);

		// Then
		Assert.Same(engine, result);
	}

	[Fact]
	public void Remove_RootIsLeaf_Success()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window.Object);

		// When
		ILayoutEngine result = engine.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window.Object));
		Assert.Equal(0, result.Count);
	}

	[Fact]
	public void Remove_RootIsLeaf_WrongWindow()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window.Object);

		Mock<IWindow> wrongWindow = new();

		// When
		ILayoutEngine result = engine.RemoveWindow(wrongWindow.Object);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_CannotFindWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object);

		Mock<IWindow> wrongWindow = new();

		// When
		ILayoutEngine result = engine.RemoveWindow(wrongWindow.Object);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.Equal(2, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_ReplaceRoot()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object);

		// When
		ILayoutEngine result = engine.RemoveWindow(window1.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_ReplaceChildSplitNodeWithWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.MoveWindowToPoint(window3.Object, new Point<double>() { X = 0.75, Y = 0.75 });

		// When
		ILayoutEngine result = engine.RemoveWindow(window3.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.False(result.ContainsWindow(window3.Object));
		Assert.Equal(2, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_ReplaceRootWithSplitNode()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.MoveWindowToPoint(window3.Object, new Point<double>() { X = 0.75, Y = 0.75 });

		// When
		ILayoutEngine result = engine.RemoveWindow(window1.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.True(result.ContainsWindow(window3.Object));
		Assert.Equal(2, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_KeepRoot()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.AddWindow(window3.Object);

		// When
		ILayoutEngine result = engine.RemoveWindow(window2.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.False(result.ContainsWindow(window2.Object));
		Assert.True(result.ContainsWindow(window3.Object));
		Assert.Equal(2, result.Count);
	}
}
