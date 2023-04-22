using Moq;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// This contains a <see cref="TreeLayoutEngine" />, with the dimensions matching <see cref="TestTree"/>.
/// </summary>
internal class TestTreeEngineEmptyMocks
{
	public Mock<IMonitor> Monitor = new();
	public Mock<IMonitorManager> MonitorManager = new();
	public Mock<IWorkspace> ActiveWorkspace = new();
	public Mock<IWorkspaceManager> WorkspaceManager = new();
	public Mock<IWindowManager> WindowManager = new();
	public Mock<IContext> Context = new();

	public TreeLayoutEngine Engine;

	public TestTreeEngineEmptyMocks()
	{
		Monitor.Setup(m => m.WorkingArea.Width).Returns(1920);
		Monitor.Setup(m => m.WorkingArea.Height).Returns(1080);
		MonitorManager.Setup(m => m.FocusedMonitor).Returns(Monitor.Object);
		Context.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);

		WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(ActiveWorkspace.Object);
		Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		Context.Setup(x => x.WindowManager).Returns(WindowManager.Object);

		Engine = new TreeLayoutEngine(Context.Object) { AddNodeDirection = Direction.Right };
	}

	public void SplitFocusedWindowWrapper(IContext context, IWindow? focusedWindow = null)
	{
		Mock<IWindow> windowModel = new();
		PhantomNode phantomNode = new WrapPhantomNode(context, windowModel.Object);
		Engine.SplitFocusedWindow(focusedWindow, phantomNode);
	}
}
