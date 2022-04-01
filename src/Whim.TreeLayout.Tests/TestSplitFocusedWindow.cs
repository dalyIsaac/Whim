using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSplitFocusedWindow
{
	[Fact]
	public void No_Focused_Window()
	{
		TestTreeEngineEmpty testTreeEngine = new(true);
		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();

		Assert.True(testTreeEngine.Engine.Root is PhantomNode);
	}

	[Fact]
	public void Add_Single_Phantom()
	{
		TestTreeEngineEmpty testTreeEngine = new(true);
		testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		testTreeEngine.Engine.Add(window1.Object);
		testTreeEngine.ActiveWorkspace.Setup(w => w.LastFocusedWindow).Returns(window1.Object);
		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();

		SplitNode root = (testTreeEngine.Engine.Root as SplitNode)!;

		var left = root[0];
		var right = root[1];

		Assert.Equal(0.5d, left.weight);
		Assert.Equal(0.5d, right.weight);

		Assert.True(left.node is WindowNode);
		Assert.True(right.node is PhantomNode);
	}

	[Fact]
	public void Add_Multiple_Phantom()
	{
		TestTreeEngineEmpty testTreeEngine = new(true);
		testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		testTreeEngine.Engine.Add(window1.Object);
		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();

		testTreeEngine.ActiveWorkspace.Setup(a => a.LastFocusedWindow).Returns(window1.Object);
		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();

		SplitNode root = (testTreeEngine.Engine.Root as SplitNode)!;
		var left = root[0];
		var middle = root[1];
		var right = root[2];

		double expectedWeight = 1d / 3d;
		Assert.Equal(expectedWeight, left.weight);
		Assert.Equal(expectedWeight, middle.weight);
		Assert.Equal(expectedWeight, right.weight);

		Assert.True(left.node is WindowNode);
		Assert.True(middle.node is PhantomNode);
		Assert.True(right.node is PhantomNode);
	}

	[Fact]
	public void Add_Nested_Phantom()
	{
		TestTreeEngineEmpty testTreeEngine = new(true);
		testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		testTreeEngine.Engine.Add(window1.Object);
		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();

		SplitNode root = (testTreeEngine.Engine.Root as SplitNode)!;
		PhantomNode phantomRight = (root[1].node as PhantomNode)!;
		testTreeEngine.ActiveWorkspace.Setup(a => a.LastFocusedWindow).Returns(phantomRight.Window);
		testTreeEngine.Engine.AddNodeDirection = Direction.Down;

		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();

		var left = root[0];
		var rightParent = root[1];
		var rightTop = ((SplitNode)rightParent.node)[0];
		var rightBottom = ((SplitNode)rightParent.node)[1];

		Assert.Equal(0.5d, left.weight);
		Assert.Equal(0.5d, rightParent.weight);
		Assert.Equal(0.5d, rightTop.weight);
		Assert.Equal(0.5d, rightBottom.weight);

		Assert.True(left.node is WindowNode);
		Assert.True(rightParent.node is SplitNode);
		Assert.True(rightTop.node is PhantomNode);
		Assert.True(rightBottom.node is PhantomNode);
	}

	[Fact]
	public void Replace_Phantom()
	{
		TestTreeEngineEmpty testTreeEngine = new(true);
		testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		testTreeEngine.Engine.Add(window1.Object);

		(testTreeEngine.Engine as WrapTreeLayoutEngine)!.SplitFocusedWindowWrapper();
		testTreeEngine.ActiveWorkspace.Setup(a => a.LastFocusedWindow).Returns(window1.Object);

		testTreeEngine.Engine.Add(window2.Object);

		SplitNode root = (testTreeEngine.Engine.Root as SplitNode)!;
		Assert.Equal(0.5d, root[0].weight);
		Assert.Equal(0.5d, root[1].weight);
		Assert.Equal(window1.Object, ((WindowNode)(root[0].node)).Window);
		Assert.Equal(window2.Object, ((WindowNode)(root[1].node)).Window);
	}
}
