using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestFlipAndMerge
{
	private readonly Mock<IWorkspace> _activeWorkspace = new();
	private readonly Mock<IWorkspaceManager> _workspaceManager = new();
	private readonly Mock<IMonitor> _focusedMonitor = new();
	private readonly Mock<IMonitorManager> _monitorManager = new();
	private readonly Mock<IConfigContext> _configContext = new();
	private readonly TreeLayoutEngine _engine;

	public TestFlipAndMerge()
	{
		_workspaceManager.Setup(x => x.ActiveWorkspace).Returns(_activeWorkspace.Object);
		_monitorManager.Setup(x => x.FocusedMonitor).Returns(_focusedMonitor.Object);

		_configContext.Setup(x => x.WorkspaceManager).Returns(_workspaceManager.Object);
		_configContext.Setup(x => x.MonitorManager).Returns(_monitorManager.Object);

		_engine = new(_configContext.Object);
	}

	[Fact]
	public void FlipAndMerge()
	{
		_engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> left = new();
		Mock<IWindow> right1 = new();
		Mock<IWindow> right2 = new();
		Mock<IWindow> right3 = new();

		_engine.Add(left.Object);
		_engine.AddNodeDirection = Direction.Right;

		_engine.Add(right1.Object);
		_engine.AddNodeDirection = Direction.Down;
		_workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right1.Object);

		_engine.Add(right2.Object);
		_workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right2.Object);
		_workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(right2.Object);

		_engine.Add(right3.Object);

		_engine.FlipAndMerge();

		SplitNode root = (SplitNode)_engine.Root!;
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
		_engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		_engine.Add(window1.Object);
		_engine.AddNodeDirection = Direction.Right;

		_engine.Add(window2.Object);
		_workspaceManager.Setup(w => w.ActiveWorkspace.LastFocusedWindow).Returns(window2.Object);

		_engine.Add(window3.Object);

		_engine.FlipAndMerge();

		SplitNode root = (SplitNode)_engine.Root!;
		Assert.Equal(3, root.Count);

		double expectedWeight = 1d / 3;
		Assert.Equal(expectedWeight, root[0].weight);
		Assert.Equal(expectedWeight, root[1].weight);
		Assert.Equal(expectedWeight, root[2].weight);
	}
}
