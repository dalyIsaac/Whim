using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestFlipAndMerge
{
	[Fact]
	public void FlipAndMerge()
	{
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IConfigContext> configContext = new();

		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);
		engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> left = new();
		Mock<IWindow> right1 = new();
		Mock<IWindow> right2 = new();
		Mock<IWindow> right3 = new();

		engine.Add(left.Object);
		engine.AddNodeDirection = Direction.Right;

		engine.Add(right1.Object);
		engine.AddNodeDirection = Direction.Down;
		workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right1.Object);

		engine.Add(right2.Object);
		workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right2.Object);
		workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right2.Object);

		engine.Add(right3.Object);

		engine.FlipAndMerge();

		SplitNode root = (SplitNode)engine.Root!;
		Assert.Equal(4, root.Count);
		Assert.Equal(0.5, root[0].weight);

		double rightWeight = 0.5d / 3;
		Assert.Equal(rightWeight, root[1].weight);
		Assert.Equal(rightWeight, root[2].weight);
		Assert.Equal(rightWeight, root[3].weight);
	}

	[Fact]
	public void FlipAndMerge_Root()
	{
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IConfigContext> configContext = new();

		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);
		engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		engine.Add(window1.Object);
		engine.AddNodeDirection = Direction.Right;

		engine.Add(window2.Object);
		workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(window2.Object);

		engine.Add(window3.Object);

		engine.FlipAndMerge();

		SplitNode root = (SplitNode)engine.Root!;
		Assert.Equal(3, root.Count);

		double expectedWeight = 1d / 3;
		Assert.Equal(expectedWeight, root[0].weight);
		Assert.Equal(expectedWeight, root[1].weight);
		Assert.Equal(expectedWeight, root[2].weight);
	}
}
