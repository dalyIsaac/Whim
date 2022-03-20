using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestFlipAndMerge
{
	private readonly TestTreeEngineEmpty _testTreeEngine = new();

	[Fact]
	public void FlipAndMerge()
	{
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> left = new();
		Mock<IWindow> right1 = new();
		Mock<IWindow> right2 = new();
		Mock<IWindow> right3 = new();

		_testTreeEngine.Engine.Add(left.Object);
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		_testTreeEngine.Engine.Add(right1.Object);
		_testTreeEngine.Engine.AddNodeDirection = Direction.Down;
		_testTreeEngine.WorkspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right1.Object);

		_testTreeEngine.Engine.Add(right2.Object);
		_testTreeEngine.WorkspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right2.Object);
		_testTreeEngine.WorkspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right2.Object);

		_testTreeEngine.Engine.Add(right3.Object);

		_testTreeEngine.Engine.FlipAndMerge();

		SplitNode root = (SplitNode)_testTreeEngine.Engine.Root!;
		Assert.Equal(4, root.Count);
		Assert.Equal(0.5, root[0].weight);

		double rightWeight = 0.5d / 3;
		for (int i = 1; i <= 3; i++)
		{
			Assert.Equal(rightWeight, root[i].weight);
			Assert.Equal(root, root[i].node.Parent);
		}
	}

	[Fact]
	public void FlipAndMerge_Root()
	{
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		_testTreeEngine.Engine.Add(window1.Object);
		_testTreeEngine.Engine.AddNodeDirection = Direction.Right;

		_testTreeEngine.Engine.Add(window2.Object);
		_testTreeEngine.WorkspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(window2.Object);

		_testTreeEngine.Engine.Add(window3.Object);

		_testTreeEngine.Engine.FlipAndMerge();

		SplitNode root = (SplitNode)_testTreeEngine.Engine.Root!;
		Assert.Equal(3, root.Count);

		double expectedWeight = 1d / 3;
		Assert.Equal(expectedWeight, root[0].weight);
		Assert.Equal(expectedWeight, root[1].weight);
		Assert.Equal(expectedWeight, root[2].weight);
	}
}
