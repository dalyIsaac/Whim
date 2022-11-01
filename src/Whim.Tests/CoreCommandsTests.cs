using Moq;
using Xunit;

namespace Whim.Tests;

public class CoreCommandsTests
{
	private static (Mock<IConfigContext>, Mock<IWorkspace>, Mock<IWindow>) CreateMocks()
	{
		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IWorkspace> workspace = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Mock<IWindow> window = new();

		workspace.SetupGet(w => w.LastFocusedWindow).Returns(window.Object);
		workspace.SetupGet(w => w.ActiveLayoutEngine).Returns(layoutEngine.Object);
		workspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(workspace.Object);
		configContext.SetupGet(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		return (configContext, workspace, window);
	}


	[Fact]
	public void FocusWindowInDirectionLeft()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionLeft.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Left, window.Object), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirectionRight()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionRight.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Right, window.Object), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirectionUp()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionUp.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Up, window.Object), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirectionDown()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionDown.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Down, window.Object), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirectionLeft()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionLeft.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Left, null), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirectionRight()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionRight.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Right, null), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirectionUp()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionUp.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Up, null), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirectionDown()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionDown.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Down, null), Times.Once);
	}

	[Fact]
	public void MoveWindowToPreviousMonitor()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowToPreviousMonitor.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.MoveWindowToPreviousMonitor(null), Times.Once);
	}

	[Fact]
	public void MoveWindowToNextMonitor()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowToNextMonitor.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.MoveWindowToNextMonitor(null), Times.Once);
	}

	[Fact]
	public void CloseCurrentWorkspace()
	{
		(Mock<IConfigContext> configContext, Mock<IWorkspace> workspace, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.CloseCurrentWorkspace.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.Remove(workspace.Object), Times.Once);
	}

	[Fact]
	public void ExitWhim()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.ExitWhim.Command.TryExecute();
		configContext.Verify(x => x.Exit(null), Times.Once);
	}
}
