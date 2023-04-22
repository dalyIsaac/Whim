using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestAddWindowAtPoint
{
	[Fact]
	public void NullRoot()
	{
		TestTreeEngineEmptyMocks emptyEngine = new();
		Mock<IWindow> window = new();

		emptyEngine.Engine.AddWindowAtPoint(window.Object, new Point<double>() { X = 0, Y = 0 });

		Assert.True(emptyEngine.Engine.Root is WindowNode);
	}

	[Fact]
	public void InvalidPoint()
	{
		TestTreeEngineEmptyMocks emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>() { X = -10, Y = -10 });

		Assert.Single(emptyEngine.Engine);
	}

	[Fact]
	public void NoParent()
	{
		TestTreeEngineEmptyMocks emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>() { X = 0.5, Y = 0.5 });

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(2, emptyEngine.Engine.Count);
	}

	[Fact]
	public void Right()
	{
		TestTreeEngineEmptyMocks emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>() { X = 0.5, Y = 0.5 });

		emptyEngine.Engine.AddNodeDirection = Direction.Left;
		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>() { X = 0.75, Y = 0.75 });

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
		TestTreeEngineEmptyMocks emptyEngine = new();
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>() { X = 0.5, Y = 0.5 });

		emptyEngine.Engine.AddNodeDirection = Direction.Right;
		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>() { X = 0.6, Y = 0.6 });

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
		TestTreeEngineEmptyMocks emptyEngine = new();
		emptyEngine.Engine.AddNodeDirection = Direction.Down;
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>() { X = 0.5, Y = 0.5 });

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>() { X = 0.6, Y = 0.6 });

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
		TestTreeEngineEmptyMocks emptyEngine = new();
		emptyEngine.Engine.AddNodeDirection = Direction.Up;
		Mock<IWindow> rootWindow = new();
		emptyEngine.Engine.AddWindow(rootWindow.Object);

		Mock<IWindow> windowWithParent = new();
		emptyEngine.Engine.AddWindowAtPoint(windowWithParent.Object, new Point<double>() { X = 0.5, Y = 0.5 });

		Mock<IWindow> pointWindow = new();
		emptyEngine.Engine.AddWindowAtPoint(pointWindow.Object, new Point<double>() { X = 0.75, Y = 0.75 });

		Assert.True(emptyEngine.Engine.Root is SplitNode);
		Assert.Equal(3, emptyEngine.Engine.Count);

		SplitNode rootNode = (SplitNode)emptyEngine.Engine.Root!;
		Assert.Equal(pointWindow.Object, (rootNode[2].node as LeafNode)!.Window);

		// Check the default direction is retained.
		Assert.Equal(Direction.Up, emptyEngine.Engine.AddNodeDirection);
	}
}
