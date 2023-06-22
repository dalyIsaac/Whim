using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// This contains a populated <see cref="TreeLayoutEngine" />, with the dimensions matching <see cref="TestTree"/>.
/// </summary>
internal class TestTreeEngineMocks
{
	public Mock<IMonitor> Monitor = new();
	public Mock<IMonitorManager> MonitorManager = new();
	public Mock<IWorkspace> ActiveWorkspace = new();
	public Mock<IWorkspaceManager> WorkspaceManager = new();
	public Mock<IWindowManager> WindowManager = new();
	public Mock<IContext> Context = new();

	public SplitNode RootNode;
	public SplitNode RightNode;
	public SplitNode RightTopNode;
	public SplitNode RightTopLeftNode;
	public SplitNode RightTopLeftBottomNode;
	public SplitNode RightTopLeftBottomRightNode;
	public SplitNode RightTopRightNode;

	public Mock<IWindow> LeftWindow = new();
	public WindowNode LeftNode;

	public Mock<IWindow> RightTopLeftTopWindow = new();
	public WindowNode RightTopLeftTopNode;

	public Mock<IWindow> RightBottomWindow = new();
	public WindowNode RightBottomNode;

	public Mock<IWindow> RightTopRight1Window = new();
	public WindowNode RightTopRight1Node;

	public Mock<IWindow> RightTopRight2Window = new();
	public WindowNode RightTopRight2Node;

	public Mock<IWindow> RightTopRight3Window = new();
	public WindowNode RightTopRight3Node;

	public Mock<IWindow> RightTopLeftBottomLeftWindow = new();
	public WindowNode RightTopLeftBottomLeftNode;

	public Mock<IWindow> RightTopLeftBottomRightTopWindow = new();
	public WindowNode RightTopLeftBottomRightTopNode;

	public Mock<IWindow> RightTopLeftBottomRightBottomWindow = new();
	public WindowNode RightTopLeftBottomRightBottomNode;

	public ILocation<int> DoLayoutLocation = new Location<int>()
	{
		X = 0,
		Y = 0,
		Width = 1920,
		Height = 1080
	};
	public TreeLayoutEngine Engine;

	public TestTreeEngineMocks()
	{
		Monitor.Setup(m => m.WorkingArea.Width).Returns(1920);
		Monitor.Setup(m => m.WorkingArea.Height).Returns(1080);
		MonitorManager.Setup(m => m.ActiveMonitor).Returns(Monitor.Object);
		Context.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);

		WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(ActiveWorkspace.Object);
		Context.Setup(x => x.WindowManager).Returns(WindowManager.Object);
		Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

		Engine = new TreeLayoutEngine(Context.Object) { AddNodeDirection = Direction.Right };
		Mock<IMonitor> doLayoutMonitor = new();
		IWindowState[] _ = Engine.DoLayout(DoLayoutLocation, doLayoutMonitor.Object).ToArray();

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
		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(LeftWindow.Object);

		RightTopLeftTopNode = Engine.AddWindow(RightTopLeftTopWindow.Object)!;
		Assert.NotNull(RightTopLeftTopNode);
		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopLeftTopWindow.Object);

		Engine.AddNodeDirection = Direction.Down;
		RightBottomNode = Engine.AddWindow(RightBottomWindow.Object)!;
		Assert.NotNull(RightBottomNode);

		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopLeftTopWindow.Object);
		Engine.AddNodeDirection = Direction.Right;

		RightTopRight1Node = Engine.AddWindow(RightTopRight1Window.Object)!;
		Assert.NotNull(RightTopRight1Node);
		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopRight1Window.Object);
		Engine.AddNodeDirection = Direction.Down;

		RightTopRight2Node = Engine.AddWindow(RightTopRight2Window.Object)!;
		Assert.NotNull(RightTopRight2Node);
		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopRight2Window.Object);

		RightTopRight3Node = Engine.AddWindow(RightTopRight3Window.Object)!;
		Assert.NotNull(RightTopRight3Node);

		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopLeftTopWindow.Object);
		Engine.AddNodeDirection = Direction.Down;

		RightTopLeftBottomLeftNode = Engine.AddWindow(RightTopLeftBottomLeftWindow.Object)!;
		Assert.NotNull(RightTopLeftBottomLeftNode);
		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopLeftBottomLeftWindow.Object);
		Engine.AddNodeDirection = Direction.Right;

		RightTopLeftBottomRightTopNode = Engine.AddWindow(RightTopLeftBottomRightTopWindow.Object)!;
		Assert.NotNull(RightTopLeftBottomRightTopNode);
		ActiveWorkspace.Setup(x => x.LastFocusedWindow).Returns(RightTopLeftBottomRightTopWindow.Object);
		Engine.AddNodeDirection = Direction.Down;

		RightTopLeftBottomRightBottomNode = Engine.AddWindow(RightTopLeftBottomRightBottomWindow.Object)!;
		Assert.NotNull(RightTopLeftBottomRightBottomNode);

		RootNode = (Engine.Root as SplitNode)!;

		RightTopLeftBottomRightNode = RightTopLeftBottomRightBottomNode.Parent!;
		RightTopLeftBottomNode = RightTopLeftBottomRightNode.Parent!;
		RightTopLeftNode = RightTopLeftBottomNode.Parent!;
		RightTopRightNode = RightTopRight1Node.Parent!;
		RightTopNode = RightTopRightNode.Parent!;
		RightNode = RightBottomNode.Parent!;

		Engine.MoveWindowEdgeInDirection(Direction.Down, 54, RightTopLeftBottomRightTopWindow.Object);
	}

	public IWindow[] GetWindows()
	{
		return new[]
		{
			LeftWindow.Object,
			RightTopLeftTopWindow.Object,
			RightBottomWindow.Object,
			RightTopRight1Window.Object,
			RightTopRight2Window.Object,
			RightTopRight3Window.Object,
			RightTopLeftBottomLeftWindow.Object,
			RightTopLeftBottomRightTopWindow.Object,
			RightTopLeftBottomRightBottomWindow.Object
		};
	}

	public void SplitFocusedWindowWrapper(IContext context, IWindow? focusedWindow = null)
	{
		Mock<IWindow> windowModel = new();
		PhantomNode phantomNode = new WrapPhantomNode(context, windowModel.Object);
		Engine.SplitFocusedWindow(focusedWindow, phantomNode);
	}
}
