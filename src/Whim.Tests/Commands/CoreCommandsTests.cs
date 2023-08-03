using Moq;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class CoreCommandsTests
{
	private class MocksWrapper
	{
		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<IWorkspace> Workspace { get; }
		public Mock<ILayoutEngine> LayoutEngine { get; }
		public Mock<IWindow> Window { get; }

		public MocksWrapper()
		{
			Context = new Mock<IContext>();
			WorkspaceManager = new Mock<IWorkspaceManager>();
			Workspace = new Mock<IWorkspace>();
			LayoutEngine = new Mock<ILayoutEngine>();
			Window = new Mock<IWindow>();

			Workspace.SetupGet(x => x.LastFocusedWindow).Returns(Window.Object);
			Workspace.SetupGet(x => x.ActiveLayoutEngine).Returns(LayoutEngine.Object);
			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		}
	}

	[Fact]
	public void ActivatePrevoiusWorkspace()
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.activate_previous_workspace");

		// When
		command.TryExecute();

		// Then
		mocks.WorkspaceManager.Verify(x => x.ActivatePrevious(null), Times.Once);
	}

	[Fact]
	public void ActivateNextWorkspace()
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.activate_next_workspace");

		// When
		command.TryExecute();

		// Then
		mocks.WorkspaceManager.Verify(x => x.ActivateNext(null), Times.Once);
	}

	[InlineData("whim.core.focus_window_in_direction.left", Direction.Left)]
	[InlineData("whim.core.focus_window_in_direction.right", Direction.Right)]
	[InlineData("whim.core.focus_window_in_direction.up", Direction.Up)]
	[InlineData("whim.core.focus_window_in_direction.down", Direction.Down)]
	[Theory]
	public void FocusWindowInDirection(string commandName, Direction direction)
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(direction, mocks.Window.Object),
			Times.Once
		);
	}

	[Fact]
	public void FocusWindowInDirection_NoLastFocusedWindow()
	{
		// Given
		MocksWrapper mocks = new();
		mocks.Workspace.SetupGet(x => x.LastFocusedWindow).Returns((IWindow?)null);
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.focus_window_in_direction.left");

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.FocusWindowInDirection(Direction.Left, null),
			Times.Never
		);
	}

	[InlineData("whim.core.swap_window_in_direction.left", Direction.Left)]
	[InlineData("whim.core.swap_window_in_direction.right", Direction.Right)]
	[InlineData("whim.core.swap_window_in_direction.up", Direction.Up)]
	[InlineData("whim.core.swap_window_in_direction.down", Direction.Down)]
	[Theory]
	public void SwapWindowInDirection(string commandName, Direction direction)
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(
			x => x.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction, null),
			Times.Once
		);
	}

	[InlineData("whim.core.move_window_left_edge_left", Direction.Left, 1, 0)]
	[InlineData("whim.core.move_window_left_edge_right", Direction.Left, -1, 0)]
	[InlineData("whim.core.move_window_right_edge_left", Direction.Right, -1, 0)]
	[InlineData("whim.core.move_window_right_edge_right", Direction.Right, 1, 0)]
	[InlineData("whim.core.move_window_top_edge_up", Direction.Up, 0, 1)]
	[InlineData("whim.core.move_window_top_edge_down", Direction.Up, 0, -1)]
	[InlineData("whim.core.move_window_bottom_edge_up", Direction.Down, 0, -1)]
	[InlineData("whim.core.move_window_bottom_edge_down", Direction.Down, 0, 1)]
	[Theory]
	public void MoveWindowEdgesInDirection(string commandName, Direction direction, int x, int y)
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);
		IPoint<int> pixelsDeltas = new Point<int>()
		{
			X = x * CoreCommands.MoveWindowEdgeDelta,
			Y = y * CoreCommands.MoveWindowEdgeDelta
		};

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(
			x => x.WorkspaceManager.MoveWindowEdgesInDirection(direction, pixelsDeltas, null),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowToPreviousMonitor()
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.move_window_to_previous_monitor");

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(x => x.WorkspaceManager.MoveWindowToPreviousMonitor(null), Times.Once);
	}

	[Fact]
	public void MoveWindowToNextMonitor()
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.move_window_to_next_monitor");

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(x => x.WorkspaceManager.MoveWindowToNextMonitor(null), Times.Once);
	}

	[Fact]
	public void CloseCurrentWorkspace()
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.close_current_workspace");

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(x => x.WorkspaceManager.Remove(mocks.Workspace.Object), Times.Once);
	}

	[Fact]
	public void ExitWhim()
	{
		// Given
		MocksWrapper mocks = new();
		CoreCommands commands = new(mocks.Context.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.exit_whim");

		// When
		command.TryExecute();

		// Then
		mocks.Context.Verify(x => x.Exit(null), Times.Once);
	}
}
