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
	public void FocusWindowInDirectionLeftCommand()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionLeft.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Left, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirectionRightCommand()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionRight.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Right, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirectionUpCommand()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionUp.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Up, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirectionDownCommand()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.FocusWindowInDirectionDown.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Down, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void SwapWindowInDirectionLeftCommand()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionLeft.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Left, null),
			Times.Once
		);
	}

	[Fact]
	public void SwapWindowInDirectionRightCommand()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionRight.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Right, null),
			Times.Once
		);
	}

	[Fact]
	public void SwapWindowInDirectionUpCommand()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionUp.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Up, null),
			Times.Once
		);
	}

	[Fact]
	public void SwapWindowInDirectionDownCommand()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.SwapWindowInDirectionDown.Command.TryExecute();
		configContext.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Down, null),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowLeftEdgeLeft()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowLeftEdgeLeft.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Left,
					CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowLeftEdgeRight()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowLeftEdgeRight.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Left,
					-CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowRightEdgeLeft()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowRightEdgeLeft.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Right,
					-CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowRightEdgeRight()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowRightEdgeRight.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Right,
					CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowTopEdgeUp()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowTopEdgeUp.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Up,
					CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowTopEdgeDown()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowTopEdgeDown.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Up,
					-CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowBottomEdgeUp()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowBottomEdgeUp.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Down,
					-CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowBottomEdgeDown()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowBottomEdgeDown.Command.TryExecute();
		configContext.Verify(
			x =>
				x.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
					Direction.Down,
					CoreCommands.MoveWindowEdgeDelta,
					null
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowToPreviousMonitorCommand()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowToPreviousMonitor.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.MoveWindowToPreviousMonitor(null), Times.Once);
	}

	[Fact]
	public void MoveWindowToNextMonitorCommand()
	{
		(Mock<IConfigContext> configContext, _, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.MoveWindowToNextMonitor.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.MoveWindowToNextMonitor(null), Times.Once);
	}

	[Fact]
	public void CloseCurrentWorkspaceCommand()
	{
		(Mock<IConfigContext> configContext, Mock<IWorkspace> workspace, _) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.CloseCurrentWorkspace.Command.TryExecute();
		configContext.Verify(x => x.WorkspaceManager.Remove(workspace.Object), Times.Once);
	}

	[Fact]
	public void ExitWhimCommand()
	{
		(Mock<IConfigContext> configContext, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(configContext.Object);

		commands.ExitWhim.Command.TryExecute();
		configContext.Verify(x => x.Exit(null), Times.Once);
	}
}
