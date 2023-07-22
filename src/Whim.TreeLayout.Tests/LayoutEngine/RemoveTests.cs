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
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		ILayoutEngine result = engine.Remove(window.Object);

		// Then
		Assert.Same(engine, result);
	}

	[Fact]
	public void Remove_RootIsLeaf_Success()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(window.Object);

		// When
		ILayoutEngine result = engine.Remove(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.Contains(window.Object));
		Assert.Equal(0, result.Count);
	}

	[Fact]
	public void Remove_RootIsLeaf_WrongWindow()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(window.Object);

		Mock<IWindow> wrongWindow = new();

		// When
		ILayoutEngine result = engine.Remove(wrongWindow.Object);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_CannotFindWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		Mock<IWindow> wrongWindow = new();

		// When
		ILayoutEngine result = engine.Remove(wrongWindow.Object);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.Equal(2, result.Count);
	}

	[Fact]
	public void Remove_RootIsSplitNode_ReplaceRoot()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		// When
		ILayoutEngine result = engine.Remove(window1.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
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
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.AddAtPoint(window3.Object, new Point<double>() { X = 0.75, Y = 0.75 });

		// When
		ILayoutEngine result = engine.Remove(window3.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.False(result.Contains(window3.Object));
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
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.AddAtPoint(window3.Object, new Point<double>() { X = 0.75, Y = 0.75 });

		// When
		ILayoutEngine result = engine.Remove(window1.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.True(result.Contains(window3.Object));
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
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

		// When
		ILayoutEngine result = engine.Remove(window2.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.False(result.Contains(window2.Object));
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(2, result.Count);
	}
}
