using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// This is a populated TreeLayoutEngine, with the dimensions matching <see cref="TestTree"/>.
/// </summary>
internal class TestTreeEngine
{
	public Mock<IMonitor> Monitor = new();
	public Mock<IMonitorManager> MonitorManager = new();
	public Mock<IWorkspace> ActiveWorkspace = new();
	public Mock<IWorkspaceManager> WorkspaceManager = new();
	public Mock<IConfigContext> ConfigContext = new();

	public Mock<IWindow> LeftWindow = new();
	public LeafNode LeftNode;

	public Mock<IWindow> RightTopLeftTopWindow = new();
	public LeafNode RightTopLeftTopNode;

	public Mock<IWindow> RightBottomWindow = new();
	public LeafNode RightBottomNode;

	public Mock<IWindow> RightTopRight1Window = new();
	public LeafNode RightTopRight1Node;

	public Mock<IWindow> RightTopRight2Window = new();
	public LeafNode RightTopRight2Node;

	public Mock<IWindow> RightTopRight3Window = new();
	public LeafNode RightTopRight3Node;

	public Mock<IWindow> RightTopLeftBottomLeftWindow = new();
	public LeafNode RightTopLeftBottomLeftNode;

	public Mock<IWindow> RightTopLeftBottomRightTopWindow = new();
	public LeafNode RightTopLeftBottomRightTopNode;

	public Mock<IWindow> RightTopLeftBottomRightBottomWindow = new();
	public LeafNode RightTopLeftBottomRightBottomNode;
	public TreeLayoutEngine Engine;

	public TestTreeEngine()
	{
		Monitor.Setup(m => m.Width).Returns(1920);
		Monitor.Setup(m => m.Height).Returns(1080);
		MonitorManager.Setup(m => m.FocusedMonitor).Returns(Monitor.Object);
		ConfigContext.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);

		WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(ActiveWorkspace.Object);
		ConfigContext.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

		Engine = new(ConfigContext.Object);
		Engine.Direction = NodeDirection.Right;

		LeftWindow.Setup(m => m.ToString()).Returns("leftWindow");
		RightTopLeftTopWindow.Setup(m => m.ToString()).Returns("rightTopLeftTopWindow");
		RightBottomWindow.Setup(m => m.ToString()).Returns("rightBottomWindow");
		RightTopRight1Window.Setup(m => m.ToString()).Returns("rightTopRight1Window");
		RightTopRight2Window.Setup(m => m.ToString()).Returns("rightTopRight2Window");
		RightTopRight3Window.Setup(m => m.ToString()).Returns("rightTopRight3Window");
		RightTopLeftBottomLeftWindow.Setup(m => m.ToString()).Returns("rightTopLeftBottomLeftWindow");
		RightTopLeftBottomRightTopWindow.Setup(m => m.ToString()).Returns("rightTopLeftBottomRightTopWindow");
		RightTopLeftBottomRightBottomWindow.Setup(m => m.ToString()).Returns("rightTopLeftBottomRightBottomWindow");

		LeftNode = Engine.AddWindow(LeftWindow.Object)!;
		Assert.NotNull(LeftNode);
		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(LeftWindow.Object);

		RightTopLeftTopNode = Engine.AddWindow(RightTopLeftTopWindow.Object)!;
		Assert.NotNull(RightTopLeftTopNode);
		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopLeftTopWindow.Object);

		Engine.Direction = NodeDirection.Down;
		RightBottomNode = Engine.AddWindow(RightBottomWindow.Object)!;
		Assert.NotNull(RightBottomNode);

		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopLeftTopWindow.Object);
		Engine.Direction = NodeDirection.Right;

		RightTopRight1Node = Engine.AddWindow(RightTopRight1Window.Object)!;
		Assert.NotNull(RightTopRight1Node);
		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopRight1Window.Object);
		Engine.Direction = NodeDirection.Down;

		RightTopRight2Node = Engine.AddWindow(RightTopRight2Window.Object)!;
		Assert.NotNull(RightTopRight2Node);
		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopRight2Window.Object);

		RightTopRight3Node = Engine.AddWindow(RightTopRight3Window.Object)!;
		Assert.NotNull(RightTopRight3Node);

		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopLeftTopWindow.Object);
		Engine.Direction = NodeDirection.Down;

		RightTopLeftBottomLeftNode = Engine.AddWindow(RightTopLeftBottomLeftWindow.Object)!;
		Assert.NotNull(RightTopLeftBottomLeftNode);
		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopLeftBottomLeftWindow.Object);
		Engine.Direction = NodeDirection.Right;

		RightTopLeftBottomRightTopNode = Engine.AddWindow(RightTopLeftBottomRightTopWindow.Object)!;
		Assert.NotNull(RightTopLeftBottomRightTopNode);
		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopLeftBottomRightTopWindow.Object);
		Engine.Direction = NodeDirection.Down;

		RightTopLeftBottomRightBottomNode = Engine.AddWindow(RightTopLeftBottomRightBottomWindow.Object)!;
		Assert.NotNull(RightTopLeftBottomRightBottomNode);

		ActiveWorkspace.Setup(x => x.FocusedWindow).Returns(RightTopLeftBottomRightBottomWindow.Object);
		Engine.MoveFocusedWindowEdgeInDirection(Direction.Up, -0.075);
	}
}
