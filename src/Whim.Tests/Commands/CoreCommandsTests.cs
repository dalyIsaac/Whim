using Moq;
using Xunit;

namespace Whim.Tests;

public class CoreCommandsTests
{
	private static (Mock<IContext>, Mock<IWorkspace>, Mock<IWindow>) CreateMocks()
	{
		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IWorkspace> workspace = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Mock<IWindow> window = new();

		workspace.SetupGet(w => w.LastFocusedWindow).Returns(window.Object);
		workspace.SetupGet(w => w.ActiveLayoutEngine).Returns(layoutEngine.Object);
		workspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(workspace.Object);
		context.SetupGet(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		return (context, workspace, window);
	}

	[Fact]
	public void FocusWindowInDirectionLeftCommand()
	{
		(Mock<IContext> context, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.FocusWindowInDirectionLeft.Command.TryExecute();
		context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Left, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirectionRightCommand()
	{
		(Mock<IContext> context, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.FocusWindowInDirectionRight.Command.TryExecute();
		context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Right, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirectionUpCommand()
	{
		(Mock<IContext> context, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.FocusWindowInDirectionUp.Command.TryExecute();
		context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Up, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirectionDownCommand()
	{
		(Mock<IContext> context, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.FocusWindowInDirectionDown.Command.TryExecute();
		context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Down, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void SwapWindowInDirectionLeftCommand()
	{
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.SwapWindowInDirectionLeft.Command.TryExecute();
		context.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Left, null), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirectionRightCommand()
	{
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.SwapWindowInDirectionRight.Command.TryExecute();
		context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Right, null),
			Times.Once
		);
	}

	[Fact]
	public void SwapWindowInDirectionUpCommand()
	{
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.SwapWindowInDirectionUp.Command.TryExecute();
		context.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Up, null), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirectionDownCommand()
	{
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.SwapWindowInDirectionDown.Command.TryExecute();
		context.Verify(x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Down, null), Times.Once);
	}

	[Fact]
	public void MoveWindowLeftEdgeLeft()
	{
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowLeftEdgeLeft.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowLeftEdgeRight.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowRightEdgeLeft.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowRightEdgeRight.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowTopEdgeUp.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowTopEdgeDown.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowBottomEdgeUp.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowBottomEdgeDown.Command.TryExecute();
		context.Verify(
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
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowToPreviousMonitor.Command.TryExecute();
		context.Verify(x => x.WorkspaceManager.MoveWindowToPreviousMonitor(null), Times.Once);
	}

	[Fact]
	public void MoveWindowToNextMonitorCommand()
	{
		(Mock<IContext> context, _, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.MoveWindowToNextMonitor.Command.TryExecute();
		context.Verify(x => x.WorkspaceManager.MoveWindowToNextMonitor(null), Times.Once);
	}

	[Fact]
	public void CloseCurrentWorkspaceCommand()
	{
		(Mock<IContext> context, Mock<IWorkspace> workspace, _) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.CloseCurrentWorkspace.Command.TryExecute();
		context.Verify(x => x.WorkspaceManager.Remove(workspace.Object), Times.Once);
	}

	[Fact]
	public void ExitWhimCommand()
	{
		(Mock<IContext> context, _, Mock<IWindow> window) = CreateMocks();
		CoreCommands commands = new(context.Object);

		commands.ExitWhim.Command.TryExecute();
		context.Verify(x => x.Exit(null), Times.Once);
	}
}
