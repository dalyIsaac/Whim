using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestAdd
{
	[Fact]
	public void Add_Root()
	{
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);

		Mock<IWindow> window = new();
		engine.Add(window.Object);

		Assert.Equal(engine.Root, new LeafNode(window.Object));
	}

	[Fact]
	public void Add_TestTree()
	{
		TestTreeEngine testEngine = new();

		TestTree tree = new(
			leftWindow: testEngine.LeftWindow,
			rightTopLeftTopWindow: testEngine.RightTopLeftTopWindow,
			rightBottomWindow: testEngine.RightBottomWindow,
			rightTopRight1Window: testEngine.RightTopRight1Window,
			rightTopRight2Window: testEngine.RightTopRight2Window,
			rightTopRight3Window: testEngine.RightTopRight3Window,
			rightTopLeftBottomLeftWindow: testEngine.RightTopLeftBottomLeftWindow,
			rightTopLeftBottomRightTopWindow: testEngine.RightTopLeftBottomRightTopWindow,
			rightTopLeftBottomRightBottomWindow: testEngine.RightTopLeftBottomRightBottomWindow
		);
		Assert.Equal(tree.Root, testEngine.Engine.Root);
	}

	[Fact]
	public void Add_UnequalSplitNode()
	{
		Mock<IMonitor> monitor = new();
		Mock<IMonitorManager> monitorManager = new();
		Mock<IWorkspace> activeWorkspace = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IConfigContext> configContext = new();

		monitor.Setup(m => m.Width).Returns(1920);
		monitor.Setup(m => m.Height).Returns(1080);
		monitorManager.Setup(m => m.FocusedMonitor).Returns(monitor.Object);
		configContext.Setup(x => x.MonitorManager).Returns(monitorManager.Object);

		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(activeWorkspace.Object);
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);
		engine.Direction = NodeDirection.Right;

		SplitNode splitNode = new(NodeDirection.Right);

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		engine.Add(window1.Object);
		engine.Add(window2.Object);

		workspaceManager.Setup(w => w.ActiveWorkspace.FocusedWindow).Returns(window1.Object);
		engine.MoveFocusedWindowEdgeInDirection(Direction.Right, 0.1);

		engine.Add(window3.Object);

		SplitNode root = (engine.Root as SplitNode)!;
		Assert.Equal(0.6d, root[0].weight);
		Assert.Equal(0.2d, root[1].weight);
		Assert.Equal(0.2d, root[2].weight);
	}
}
