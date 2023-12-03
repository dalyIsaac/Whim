using System.Collections.Generic;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class CoreCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext context = fixture.Freeze<IContext>();
		IWorkspace workspace = fixture.Freeze<IWorkspace>();
		IWindow window = fixture.Freeze<IWindow>();

		context.WorkspaceManager.ActiveWorkspace.Returns(workspace);
		workspace.LastFocusedWindow.Returns(window);

		fixture.Inject(context);
		fixture.Inject(workspace);
		fixture.Inject(window);
	}
}

public class CoreCommandsTests
{
	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void ActivatePrevoiusWorkspace(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.activate_previous_workspace");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.Received(1).ActivatePrevious(null);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void ActivateNextWorkspace(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.activate_next_workspace");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.Received(1).ActivateNext(null);
	}

	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.focus_window_in_direction.left", Direction.Left)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.focus_window_in_direction.right", Direction.Right)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.focus_window_in_direction.up", Direction.Up)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.focus_window_in_direction.down", Direction.Down)]
	[Theory]
	public void FocusWindowInDirection(string commandName, Direction direction, IContext context, IWindow window)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.ActiveWorkspace.Received(1).FocusWindowInDirection(direction, window);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void FocusWindowInDirection_NoLastFocusedWindow(IContext context, IWorkspace workspace)
	{
		// Given
		workspace.LastFocusedWindow.Returns((IWindow?)null);
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.focus_window_in_direction.left");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.ActiveWorkspace.DidNotReceive().FocusWindowInDirection(Direction.Left, null);
	}

	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.swap_window_in_direction.left", Direction.Left)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.swap_window_in_direction.right", Direction.Right)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.swap_window_in_direction.up", Direction.Up)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.swap_window_in_direction.down", Direction.Down)]
	[Theory]
	public void SwapWindowInDirection(string commandName, Direction direction, IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.ActiveWorkspace.Received(1).SwapWindowInDirection(direction, null);
	}

	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.move_window_left_edge_left", Direction.Left, -1, 0)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.move_window_left_edge_right", Direction.Left, 1, 0)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>(
		"whim.core.move_window_right_edge_left",
		Direction.Right,
		-1,
		0
	)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>(
		"whim.core.move_window_right_edge_right",
		Direction.Right,
		1,
		0
	)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.move_window_top_edge_up", Direction.Up, 0, -1)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.move_window_top_edge_down", Direction.Up, 0, 1)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.move_window_bottom_edge_up", Direction.Down, 0, -1)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>(
		"whim.core.move_window_bottom_edge_down",
		Direction.Down,
		0,
		1
	)]
	[Theory]
	public void MoveWindowEdgesInDirection(string commandName, Direction direction, int x, int y, IContext context)
	{
		// Given
		CoreCommands commands = new(context);
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
		context.WorkspaceManager.Received(1).MoveWindowEdgesInDirection(direction, pixelsDeltas, null);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void MoveWindowToPreviousMonitor(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.move_window_to_previous_monitor");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.Received(1).MoveWindowToPreviousMonitor(null);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void MoveWindowToNextMonitor(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.move_window_to_next_monitor");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.Received(1).MoveWindowToNextMonitor(null);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void MaximizeWindow(IContext context, IWindow window)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);

		ICommand command = testUtils.GetCommand("whim.core.maximize_window");

		// When
		command.TryExecute();

		// Then
		window.Received(1).ShowMaximized();
	}

	#region MinimizeWindow
	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void MinimizeWindow(IContext context, IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		window3.IsMinimized.Returns(false);

		IWorkspace activeWorkspace = context.WorkspaceManager.ActiveWorkspace;
		activeWorkspace.LastFocusedWindow.Returns(window1);
		activeWorkspace.Windows.GetEnumerator().Returns(new List<IWindow>() { window2, window3 }.GetEnumerator());

		ICommand command = testUtils.GetCommand("whim.core.minimize_window");

		// When
		command.TryExecute();

		// Then
		window1.Received(1).ShowMinimized();
		window3.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void MinimizeWindow_NoLastFocusedWindow(IContext context, IWindow window1, IWindow window2)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		window2.IsMinimized.Returns(false);

		IWorkspace activeWorkspace = context.WorkspaceManager.ActiveWorkspace;
		activeWorkspace.LastFocusedWindow.Returns((IWindow?)null);
		activeWorkspace.Windows.GetEnumerator().Returns(new List<IWindow>() { window1, window2 }.GetEnumerator());

		ICommand command = testUtils.GetCommand("whim.core.minimize_window");

		// When
		command.TryExecute();

		// Then
		window2.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void MinimizeWindow_FocusFirstWindow(IContext context, IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		IWorkspace activeWorkspace = context.WorkspaceManager.ActiveWorkspace;
		activeWorkspace.LastFocusedWindow.Returns(window1);
		activeWorkspace.Windows.GetEnumerator().Returns(new List<IWindow>() { window2, window3 }.GetEnumerator());

		ICommand command = testUtils.GetCommand("whim.core.minimize_window");

		// When
		command.TryExecute();

		// Then
		window1.Received(1).ShowMinimized();
		window2.Received(1).Focus();
	}
	#endregion

	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.focus_previous_monitor")]
	[InlineAutoSubstituteData<CoreCommandsCustomization>("whim.core.focus_next_monitor")]
	[Theory]
	public void FocusMonitor(string commandName, IContext context, IWorkspace workspace)
	{
		// Given
		workspace.LastFocusedWindow.Returns((IWindow?)null);
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		workspace.LastFocusedWindow?.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void CloseCurrentWorkspace(IContext context, IWorkspace workspace)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.close_current_workspace");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.Received(1).Remove(workspace);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void ExitWhim(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.exit_whim");

		// When
		command.TryExecute();

		// Then
		context.Received(1).Exit(null);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void RestartWhim(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.restart_whim");

		// When
		command.TryExecute();

		// Then
		context.Received(1).Exit(Arg.Is<ExitEventArgs>(args => args.Reason == ExitReason.Restart));
	}

	private static List<IWorkspace> CreateWorkspaces(int count)
	{
		List<IWorkspace> workspaces = new();
		for (int idx = 0; idx < count; idx++)
		{
			workspaces.Add(Substitute.For<IWorkspace>());
		}
		return workspaces;
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void ActivateWorkspaceAtIndex_IndexDoesNotExist(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);
		List<IWorkspace> workspaces = CreateWorkspaces(2);
		context.WorkspaceManager.GetEnumerator().Returns(workspaces.GetEnumerator());

		int index = 3;

		ICommand command = testUtils.GetCommand($"whim.core.activate_workspace_{index}");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.DidNotReceive().Activate(Arg.Any<IWorkspace>());
	}

	[Theory]
	[InlineAutoSubstituteData<CoreCommandsCustomization>(1)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>(2)]
	[InlineAutoSubstituteData<CoreCommandsCustomization>(10)]
	public void ActivateWorkspaceAtIndex(int index, IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);
		List<IWorkspace> workspaces = CreateWorkspaces(10);
		context.WorkspaceManager.GetEnumerator().Returns(workspaces.GetEnumerator());

		ICommand command = testUtils.GetCommand($"whim.core.activate_workspace_{index}");

		// When
		command.TryExecute();

		// Then
		context.WorkspaceManager.Received(1).Activate(workspaces[index - 1]);
	}

	[Theory, AutoSubstituteData<CoreCommandsCustomization>]
	public void ActivateWorkspaceAtIndex_VerifyKeybinds(IContext context)
	{
		// Given
		CoreCommands commands = new(context);
		PluginCommandsTestUtils testUtils = new(commands);

		// When
		IKeybind[] keybinds = new IKeybind[10];
		for (int idx = 0; idx < 10; idx++)
		{
			keybinds[idx] = testUtils.GetKeybind($"whim.core.activate_workspace_{idx + 1}");
		}

		// Then
		for (int idx = 0; idx < 9; idx++)
		{
			Assert.Equal(KeyModifiers.LAlt | KeyModifiers.LShift, keybinds[idx].Modifiers);
			Assert.Equal("VK_" + (idx + 1), keybinds[idx].Key.ToString());
		}

		Assert.Equal(KeyModifiers.LAlt | KeyModifiers.LShift, keybinds[9].Modifiers);
		Assert.Equal("VK_0", keybinds[9].Key.ToString());
	}
}
