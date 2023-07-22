using Moq;

namespace Whim.TreeLayout.Tests;

internal class LayoutEngineWrapper
{
	public Mock<IContext> Context { get; } = new();
	public Mock<ITreeLayoutPlugin> Plugin { get; } = new();
	public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
	public Mock<IWorkspace> Workspace { get; } = new();
	public Mock<IMonitorManager> MonitorManager { get; } = new();
	public Mock<IMonitor> Monitor { get; } = new();

	public LayoutEngineWrapper()
	{
		Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow>());

		Monitor.Setup(m => m.WorkingArea).Returns(new Location<int>() { Width = 100, Height = 100 });

		WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(Workspace.Object);
		MonitorManager.Setup(x => x.ActiveMonitor).Returns(Monitor.Object);

		Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		Context.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);
	}

	public LayoutEngineWrapper SetAsPhantom(params IWindow[] windows)
	{
		Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow>(windows));
		return this;
	}

	public LayoutEngineWrapper SetAsLastFocusedWindow(IWindow? window)
	{
		Workspace.Setup(x => x.LastFocusedWindow).Returns(window);
		return this;
	}
}
