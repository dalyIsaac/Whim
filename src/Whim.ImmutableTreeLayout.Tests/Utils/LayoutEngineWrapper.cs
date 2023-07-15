using Moq;

namespace Whim.ImmutableTreeLayout.Tests;

internal class LayoutEngineWrapper
{
	public Mock<IContext> Context { get; } = new();
	public Mock<IImmutableInternalTreePlugin> Plugin { get; } = new();
	public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
	public Mock<IWorkspace> Workspace { get; } = new();

	public LayoutEngineWrapper()
	{
		Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow>());

		WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(Workspace.Object);
		Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
	}

	public LayoutEngineWrapper SetAsPhantom(IWindow window)
	{
		Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow> { window });
		return this;
	}

	public LayoutEngineWrapper SetAsLastFocusedWindow(IWindow? window)
	{
		Workspace.Setup(x => x.LastFocusedWindow).Returns(window);
		return this;
	}
}
