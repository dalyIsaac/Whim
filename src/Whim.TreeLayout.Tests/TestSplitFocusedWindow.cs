using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSplitFocusedWindow
{
	private readonly TestTreeEngineEmpty _testTreeEngine = new();

	[StaFact]
	public void No_Focused_Window()
	{
		_testTreeEngine.Engine.SplitFocusedWindow();

		Assert.True(_testTreeEngine.Engine.Root is PhantomNode);
	}

	[StaFact]
	public void Add_Single_Phantom()
	{
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		_testTreeEngine.Engine.Add(window1.Object);
		_testTreeEngine.ActiveWorkspace.Setup(w => w.LastFocusedWindow).Returns(window1.Object);
		_testTreeEngine.Engine.SplitFocusedWindow();

		SplitNode root = (_testTreeEngine.Engine.Root as SplitNode)!;

		var left = root[0];
		var right = root[1];

		Assert.Equal(0.5d, left.weight);
		Assert.Equal(0.5d, right.weight);

		Assert.True(left.node is WindowNode);
		Assert.True(right.node is PhantomNode);
	}

	[StaFact]
	public void Add_Multiple_Phantom()
	{
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		_testTreeEngine.Engine.Add(window1.Object);
		_testTreeEngine.Engine.SplitFocusedWindow();

		_testTreeEngine.ActiveWorkspace.Setup(a => a.LastFocusedWindow).Returns(window1.Object);
		_testTreeEngine.Engine.SplitFocusedWindow();

		SplitNode root = (_testTreeEngine.Engine.Root as SplitNode)!;
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

	[StaFact]
	public void Add_Nested_Phantom()
	{
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		_testTreeEngine.Engine.Add(window1.Object);
		_testTreeEngine.Engine.SplitFocusedWindow();

		SplitNode root = (_testTreeEngine.Engine.Root as SplitNode)!;
		PhantomNode phantomRight = (root[1].node as PhantomNode)!;
		_testTreeEngine.ActiveWorkspace.Setup(a => a.LastFocusedWindow).Returns(phantomRight.Window);
		_testTreeEngine.Engine.AddNodeDirection = Direction.Down;

		_testTreeEngine.Engine.SplitFocusedWindow();

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

	[StaFact]
	public void Replace_Phantom()
	{
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		_testTreeEngine.Engine.Add(window1.Object);

		_testTreeEngine.Engine.SplitFocusedWindow();
		_testTreeEngine.ActiveWorkspace.Setup(a => a.LastFocusedWindow).Returns(window1.Object);

		_testTreeEngine.Engine.Add(window2.Object);

		SplitNode root = (_testTreeEngine.Engine.Root as SplitNode)!;
		Assert.Equal(0.5d, root[0].weight);
		Assert.Equal(0.5d, root[1].weight);
		Assert.Equal(window1.Object, ((WindowNode)(root[0].node)).Window);
		Assert.Equal(window2.Object, ((WindowNode)(root[1].node)).Window);
	}
}
