using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestSplitFocusedWindow
{
	private readonly Mock<IMonitor> _monitor = new();
	private readonly Mock<IMonitorManager> _monitorManager = new();
	private readonly Mock<IWorkspace> _activeWorkspace = new();
	private readonly Mock<IWorkspaceManager> _workspaceManager = new();
	private readonly Mock<IWindowManager> _windowManager = new();
	private readonly Mock<IConfigContext> _configContext = new();
	private readonly TreeLayoutEngine _engine;

	public TestSplitFocusedWindow()
	{
		_monitor.Setup(m => m.Width).Returns(1920);
		_monitor.Setup(m => m.Height).Returns(1080);

		_monitorManager.Setup(m => m.FocusedMonitor).Returns(_monitor.Object);
		_workspaceManager.Setup(x => x.ActiveWorkspace).Returns(_activeWorkspace.Object);

		_configContext.Setup(x => x.MonitorManager).Returns(_monitorManager.Object);
		_configContext.Setup(x => x.WorkspaceManager).Returns(_workspaceManager.Object);
		_configContext.Setup(x => x.WindowManager).Returns(_windowManager.Object);

		_engine = new TreeLayoutEngine(_configContext.Object);
	}

	[StaFact]
	public void No_Focused_Window()
	{
		_engine.SplitFocusedWindow();

		Assert.True(_engine.Root is PhantomNode);
	}

	[StaFact]
	public void Add_Single_Phantom()
	{
		_engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		_engine.Add(window1.Object);
		_activeWorkspace.Setup(w => w.LastFocusedWindow).Returns(window1.Object);
		_engine.SplitFocusedWindow();

		SplitNode root = (_engine.Root as SplitNode)!;

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
		_engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		_engine.Add(window1.Object);
		_engine.SplitFocusedWindow();

		_activeWorkspace.Setup(a => a.LastFocusedWindow).Returns(window1.Object);
		_engine.SplitFocusedWindow();

		SplitNode root = (_engine.Root as SplitNode)!;
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
		_engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();

		_engine.Add(window1.Object);
		_engine.SplitFocusedWindow();

		SplitNode root = (_engine.Root as SplitNode)!;
		PhantomNode phantomRight = (root[1].node as PhantomNode)!;
		_activeWorkspace.Setup(a => a.LastFocusedWindow).Returns(phantomRight.Window);
		_engine.AddNodeDirection = Direction.Down;

		_engine.SplitFocusedWindow();

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
		_engine.AddNodeDirection = Direction.Right;

		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		_engine.Add(window1.Object);

		_engine.SplitFocusedWindow();
		_activeWorkspace.Setup(a => a.LastFocusedWindow).Returns(window1.Object);

		_engine.Add(window2.Object);

		SplitNode root = (_engine.Root as SplitNode)!;
		Assert.Equal(0.5d, root[0].weight);
		Assert.Equal(0.5d, root[1].weight);
		Assert.Equal(window1.Object, ((WindowNode)(root[0].node)).Window);
		Assert.Equal(window2.Object, ((WindowNode)(root[1].node)).Window);
	}
}
