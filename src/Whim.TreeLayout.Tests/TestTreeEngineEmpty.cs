using Moq;

namespace Whim.TreeLayout.Tests;

/// <summary>
/// This is a populated TreeLayoutEngine, with the dimensions matching <see cref="TestTree"/>.
/// </summary>
internal class TestTreeEngineEmpty
{
	public Mock<IMonitor> Monitor = new();
	public Mock<IMonitorManager> MonitorManager = new();
	public Mock<IWorkspace> ActiveWorkspace = new();
	public Mock<IWorkspaceManager> WorkspaceManager = new();
	public Mock<IWindowManager> WindowManager = new();
	public Mock<IConfigContext> ConfigContext = new();

	public TreeLayoutEngine Engine;

	public TestTreeEngineEmpty(bool testTreeLayoutEngine = false)
	{
		Monitor.Setup(m => m.WorkingArea.Width).Returns(1920);
		Monitor.Setup(m => m.WorkingArea.Height).Returns(1080);
		MonitorManager.Setup(m => m.FocusedMonitor).Returns(Monitor.Object);
		ConfigContext.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);

		WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(ActiveWorkspace.Object);
		ConfigContext.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		ConfigContext.Setup(x => x.WindowManager).Returns(WindowManager.Object);

		Engine = testTreeLayoutEngine
			? new WrapTreeLayoutEngine(ConfigContext.Object)
			: new TreeLayoutEngine(ConfigContext.Object);
		Engine.AddNodeDirection = Direction.Right;
	}
}
