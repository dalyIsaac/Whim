using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestAddWindowAtPoint
{
	// Cases to add:
	// 1. Root is null.
	// 2. Couldn't find node at point.
	// 3. There is no parent node.
	// 4. There is a parent node.
	// 5. Direction is horizontal.
	// 6. Direction is vertical.
	// 7. Adding phantom node.
	// 8. Adding window node.

	[Fact]
	public void NullRoot()
	{
		TestTreeEngineEmpty emptyEngine = new();
		Mock<IWindow> window = new();

		emptyEngine.Engine.AddWindowAtPoint(window.Object, new Point<double>(0, 0), false);

		Assert.True(emptyEngine.Engine.Root is WindowNode);
	}

	[Fact]
	public void InvalidPoint()
	{
		TestTreeEngineEmpty emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>(-10, -10), false);

		Assert.Single(emptyEngine.Engine);
	}

	[Fact]
	public void NoParent()
	{
		TestTreeEngineEmpty emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>(0.5, 0.5), false);

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(2, emptyEngine.Engine.Count);
	}

	[Fact]
	public void Right()
	{
		TestTreeEngineEmpty emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>(0.5, 0.5), false);

		emptyEngine.Engine.AddNodeDirection = Direction.Left;
		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>(0.75, 0.75), false);

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(3, emptyEngine.Engine.Count);

		SplitNode rootNode = (SplitNode)emptyEngine.Engine.Root!;
		Assert.Equal(pointWindow.Object, (rootNode[2].node as LeafNode)!.Window);

		// Check the default direction is retained.
		Assert.Equal(Direction.Left, emptyEngine.Engine.AddNodeDirection);
	}

	[Fact]
	public void Left()
	{
		TestTreeEngineEmpty emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>(0.5, 0.5), false);

		emptyEngine.Engine.AddNodeDirection = Direction.Right;
		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>(0.6, 0.6), false);

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(3, emptyEngine.Engine.Count);

		SplitNode rootNode = (SplitNode)emptyEngine.Engine.Root!;
		Assert.Equal(pointWindow.Object, (rootNode[1].node as LeafNode)!.Window);

		// Check the default direction is retained.
		Assert.Equal(Direction.Right, emptyEngine.Engine.AddNodeDirection);
	}

	[Fact]
	public void Up()
	{
		TestTreeEngineEmpty emptyEngine = new();
		emptyEngine.Engine.AddNodeDirection = Direction.Down;
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>(0.5, 0.5), false);

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>(0.6, 0.6), false);

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(3, emptyEngine.Engine.Count);

		SplitNode rootNode = (SplitNode)emptyEngine.Engine.Root!;
		Assert.Equal(pointWindow.Object, (rootNode[1].node as LeafNode)!.Window);

		// Check the default direction is retained.
		Assert.Equal(Direction.Down, emptyEngine.Engine.AddNodeDirection);
	}

	[Fact]
	public void Down()
	{
		TestTreeEngineEmpty emptyEngine = new();
		emptyEngine.Engine.AddNodeDirection = Direction.Up;
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>(0.5, 0.5), false);

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>(0.75, 0.75), false);

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(3, emptyEngine.Engine.Count);

		SplitNode rootNode = (SplitNode)emptyEngine.Engine.Root!;
		Assert.Equal(pointWindow.Object, (rootNode[2].node as LeafNode)!.Window);

		// Check the default direction is retained.
		Assert.Equal(Direction.Up, emptyEngine.Engine.AddNodeDirection);
	}
}
