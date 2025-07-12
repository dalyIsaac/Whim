using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class CoreCommandsTests
{
	[Theory, AutoSubstituteData]
	public void ActivatePreviousWorkspace(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.activate_previous_workspace");

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new ActivateAdjacentWorkspaceTransform(Reverse: true));
	}

	[Theory, AutoSubstituteData]
	public void ActivateNextWorkspace(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.activate_next_workspace");

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new ActivateAdjacentWorkspaceTransform());
	}

	[InlineAutoSubstituteData("whim.core.focus_window_in_direction.left", Direction.Left)]
	[InlineAutoSubstituteData("whim.core.focus_window_in_direction.right", Direction.Right)]
	[InlineAutoSubstituteData("whim.core.focus_window_in_direction.up", Direction.Up)]
	[InlineAutoSubstituteData("whim.core.focus_window_in_direction.down", Direction.Down)]
	[Theory]
	public void FocusWindowInDirection(string commandName, Direction direction, IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new FocusWindowInDirectionTransform(Direction: direction));
	}

	[InlineAutoSubstituteData("whim.core.swap_window_in_direction.left", Direction.Left)]
	[InlineAutoSubstituteData("whim.core.swap_window_in_direction.right", Direction.Right)]
	[InlineAutoSubstituteData("whim.core.swap_window_in_direction.up", Direction.Up)]
	[InlineAutoSubstituteData("whim.core.swap_window_in_direction.down", Direction.Down)]
	[Theory]
	public void SwapWindowInDirection(string commandName, Direction direction, IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new SwapWindowInDirectionTransform(Direction: direction));
	}

	[InlineAutoSubstituteData("whim.core.move_window_left_edge_left", Direction.Left, -1, 0)]
	[InlineAutoSubstituteData("whim.core.move_window_left_edge_right", Direction.Left, 1, 0)]
	[InlineAutoSubstituteData("whim.core.move_window_right_edge_left", Direction.Right, -1, 0)]
	[InlineAutoSubstituteData("whim.core.move_window_right_edge_right", Direction.Right, 1, 0)]
	[InlineAutoSubstituteData("whim.core.move_window_top_edge_up", Direction.Up, 0, -1)]
	[InlineAutoSubstituteData("whim.core.move_window_top_edge_down", Direction.Up, 0, 1)]
	[InlineAutoSubstituteData("whim.core.move_window_bottom_edge_up", Direction.Down, 0, -1)]
	[InlineAutoSubstituteData("whim.core.move_window_bottom_edge_down", Direction.Down, 0, 1)]
	[Theory]
	public void MoveWindowEdgesInDirection(string commandName, Direction direction, int x, int y, IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);
		IPoint<int> pixelsDeltas = new Point<int>()
		{
			X = x * CoreCommands.MoveWindowEdgeDelta,
			Y = y * CoreCommands.MoveWindowEdgeDelta,
		};

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new MoveWindowEdgesInDirectionTransform(direction, pixelsDeltas));
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPreviousMonitor(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.move_window_to_previous_monitor");

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new MoveWindowToAdjacentMonitorTransform(Reverse: true));
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToNextMonitor(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.move_window_to_next_monitor");

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new MoveWindowToAdjacentMonitorTransform());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MaximizeWindow(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);
		window.Handle.Returns((HWND)123);

		PopulateThreeWayMap(
			root,
			CreateMonitor(),
			CreateWorkspace() with
			{
				LastFocusedWindowHandle = window.Handle,
			},
			window
		);

		ICommand command = testUtils.GetCommand("whim.core.maximize_window");

		// When
		command.TryExecute();

		// Then
		window.Received(1).ShowMaximized();
	}

	#region MinimizeWindow
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindow(
		IContext ctx,
		MutableRootSector root,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		window1.Handle.Returns((HWND)123);
		window3.IsMinimized.Returns(false);

		Workspace workspace = CreateWorkspace() with { LastFocusedWindowHandle = window1.Handle };
		AddActiveWorkspaceToStore(root, workspace);
		PopulateWindowWorkspaceMap(root, window1, workspace);
		PopulateWindowWorkspaceMap(root, window2, workspace);
		PopulateWindowWorkspaceMap(root, window3, workspace);

		ICommand command = testUtils.GetCommand("whim.core.minimize_window");

		// When
		command.TryExecute();

		// Then
		window1.Received(1).ShowMinimized();
		window3.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MinimizeWindow_NoLastFocusedWindow(
		IContext ctx,
		MutableRootSector root,
		IWindow window1,
		IWindow window2
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		window2.IsMinimized.Returns(false);

		Workspace workspace = CreateWorkspace();
		AddActiveWorkspaceToStore(root, workspace);
		PopulateWindowWorkspaceMap(root, window1, workspace);
		PopulateWindowWorkspaceMap(root, window2, workspace);

		ICommand command = testUtils.GetCommand("whim.core.minimize_window");

		// When
		command.TryExecute();

		// Then
		window2.Received(1).Focus();
	}
	#endregion

	#region Cycle layouts
	[Theory, AutoSubstituteData]
	public void CycleLayoutEngine_Next(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.cycle_layout_engine.next");

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new CycleLayoutEngineTransform());
	}

	[Theory, AutoSubstituteData]
	public void CycleLayoutEngine_Previous(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.cycle_layout_engine.previous");

		// When
		command.TryExecute();

		// Then
		ctx.Store.Received(1).Dispatch(new CycleLayoutEngineTransform(Reverse: true));
	}
	#endregion


	[InlineAutoSubstituteData<StoreCustomization>("whim.core.focus_previous_monitor")]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.focus_next_monitor")]
	[Theory]
	internal void FocusMonitor(string commandName, IContext ctx, MutableRootSector root, List<object> transforms)
	{
		// Given
		Workspace w1 = CreateWorkspace();
		Workspace w2 = CreateWorkspace();

		IMonitor m1 = CreateMonitor((HMONITOR)1);
		IMonitor m2 = CreateMonitor((HMONITOR)2);

		PopulateMonitorWorkspaceMap(root, m1, w1);
		PopulateMonitorWorkspaceMap(root, m2, w2);

		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		Assert.Contains(transforms, t => t.Equals(new FocusWorkspaceTransform(w2.Id)));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusMonitor_CannotGetWorkspaceForMonitor(
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.focus_previous_monitor");

		IMonitor monitor = CreateMonitor();
		AddMonitorsToSector(root, monitor);
		root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.Add(monitor.Handle, Guid.NewGuid());

		// When
		command.TryExecute();

		// Then
		Assert.DoesNotContain(transforms, t => t is FocusWorkspaceTransform);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CloseCurrentWorkspace(IContext ctx, MutableRootSector root, List<object> transforms)
	{
		// Given there is an active workspace
		Workspace workspace = CreateWorkspace();
		AddActiveWorkspaceToStore(root, workspace);

		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.close_current_workspace");

		// When
		command.TryExecute();

		// Then
		Assert.Contains(transforms, t => t.Equals(new RemoveWorkspaceByIdTransform(workspace.Id)));
	}

	[Theory, AutoSubstituteData]
	public void ExitWhim(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.exit_whim");

		// When
		command.TryExecute();

		// Then
		ctx.Received(1).Exit(null);
	}

	[Theory, AutoSubstituteData]
	public void RestartWhim(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.core.restart_whim");

		// When
		command.TryExecute();

		// Then
		ctx.Received(1).Exit(Arg.Is<ExitEventArgs>(args => args.Reason == ExitReason.Restart));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActivateWorkspaceAtIndex_IndexDoesNotExist(
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		for (int idx = 0; idx < 2; idx++)
		{
			AddWorkspaceToStore(root, CreateWorkspace());
		}

		int index = 3;

		ICommand command = testUtils.GetCommand($"whim.core.activate_workspace_{index}");

		// When
		command.TryExecute();

		// Then
		Assert.DoesNotContain(transforms, t => t is ActivateWorkspaceTransform);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(1)]
	[InlineAutoSubstituteData<StoreCustomization>(2)]
	[InlineAutoSubstituteData<StoreCustomization>(10)]
	internal void ActivateWorkspaceAtIndex(int index, IContext ctx, MutableRootSector root, List<object> transforms)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		for (int idx = 0; idx < 10; idx++)
		{
			AddWorkspaceToStore(root, CreateWorkspace());
		}

		ICommand command = testUtils.GetCommand($"whim.core.activate_workspace_{index}");

		// When
		command.TryExecute();

		// Then
		Assert.Contains(
			transforms,
			t => t.Equals(new ActivateWorkspaceTransform(root.WorkspaceSector.WorkspaceOrder[index - 1]))
		);
	}

	[Theory, AutoSubstituteData]
	public void ActivateWorkspaceAtIndex_VerifyKeybinds(IContext ctx)
	{
		// Given
		CoreCommands commands = new(ctx);
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

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveActiveWindowToWorkspaceAtIndex_IndexDoesNotExist(
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		for (int idx = 0; idx < 2; idx++)
		{
			AddWorkspaceToStore(root, CreateWorkspace());
		}

		int index = 3;

		ICommand command = testUtils.GetCommand($"whim.core.move_active_window_to_workspace_{index}");

		// When
		command.TryExecute();

		// Then
		Assert.DoesNotContain(transforms, t => t is MoveWindowToWorkspaceTransform);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(1)]
	[InlineAutoSubstituteData<StoreCustomization>(2)]
	[InlineAutoSubstituteData<StoreCustomization>(10)]
	internal void MoveActiveWindowToWorkspaceAtIndex(
		int index,
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		IWindow window = CreateWindow((HWND)123);
		for (int idx = 0; idx < 10; idx++)
		{
			Workspace w = CreateWorkspace() with { LastFocusedWindowHandle = window.Handle };
			if (idx == 0)
			{
				w = w with { LastFocusedWindowHandle = window.Handle };
				w = PopulateWindowWorkspaceMap(root, window, w);
				AddActiveWorkspaceToStore(root, w);
			}
			else
			{
				AddWorkspaceToStore(root, w);
			}
		}

		ICommand command = testUtils.GetCommand($"whim.core.move_active_window_to_workspace_{index}");

		// When
		command.TryExecute();

		// Then
		Assert.Contains(
			transforms,
			t =>
				t.Equals(
					new MoveWindowToWorkspaceTransform(root.WorkspaceSector.WorkspaceOrder[index - 1], window.Handle)
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusLayoutToggleMaximized_NotFocusLayoutEngine(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine layoutEngine,
		List<object> transforms
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		Workspace workspace = CreateWorkspace() with { LayoutEngines = [layoutEngine] };
		AddActiveWorkspaceToStore(root, workspace);

		ICommand command = testUtils.GetCommand("whim.core.focus_layout.toggle_maximized");

		// When
		command.TryExecute();

		// Then
		Assert.DoesNotContain(transforms, t => t is LayoutEngineCustomActionTransform);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusLayoutToggleMaximized_FocusLayoutEngine(
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		FocusLayoutEngine engine = new(new LayoutEngineIdentity());
		Workspace workspace = CreateWorkspace() with { LayoutEngines = [engine] };
		AddActiveWorkspaceToStore(root, workspace);

		ICommand command = testUtils.GetCommand("whim.core.focus_layout.toggle_maximized");

		// When
		command.TryExecute();

		// Then
		Assert.Contains(
			transforms,
			t =>
				t.Equals(
					new LayoutEngineCustomActionTransform(
						workspace.Id,
						new() { Name = $"{engine.Name}.toggle_maximized", Window = null }
					)
				)
		);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.focus_next_workspace_on_current_monitor", true)]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.focus_previous_workspace_on_current_monitor", false)]
	internal void FocusWorkspaceOnCurrentMonitor(
		string commandName,
		bool getNext,
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given we have three workspaces, two sticky to monitor 1
		var workspace1 = CreateWorkspace();
		var workspace2 = CreateWorkspace();
		var workspace3 = CreateWorkspace();
		AddWorkspacesToStore(root, workspace1, workspace2, workspace3);

		IMonitor monitor = CreateMonitor((HMONITOR)1);
		PopulateMonitorWorkspaceMap(root, monitor, workspace1);
		root.MonitorSector.ActiveMonitorHandle = monitor.Handle;

		root.MapSector.StickyWorkspaceMonitorIndexMap = root
			.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(workspace1.Id, [0, 1])
			.SetItem(workspace2.Id, [0, 4]);

		// Set workspace1 as active
		AddActiveWorkspaceToStore(root, workspace1);

		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);
		command.TryExecute();

		// Then the next/previous workspace is focused
		Guid expectedWorkspaceId = getNext ? workspace2.Id : workspace3.Id;
		Assert.Contains(transforms, t => t.Equals(new ActivateWorkspaceTransform(expectedWorkspaceId)));
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.move_window_to_next_workspace_on_monitor", true)]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.move_window_to_previous_workspace_on_monitor", false)]
	internal void MoveWindowToWorkspaceOnCurrentMonitor(
		string commandName,
		bool getNext,
		IContext ctx,
		MutableRootSector root,
		List<object> transforms,
		IWindow window
	)
	{
		// Given we have three workspaces, two sticky to monitor 1
		var workspace1 = CreateWorkspace();
		var workspace2 = CreateWorkspace();
		var workspace3 = CreateWorkspace();
		AddWorkspacesToStore(root, workspace1, workspace2, workspace3);

		IMonitor monitor = CreateMonitor((HMONITOR)1);
		PopulateMonitorWorkspaceMap(root, monitor, workspace1);
		root.MonitorSector.ActiveMonitorHandle = monitor.Handle;

		root.MapSector.StickyWorkspaceMonitorIndexMap = root
			.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(workspace1.Id, [0, 1])
			.SetItem(workspace2.Id, [0, 4]);

		// Set up window in workspace1 as last focused
		window.Handle.Returns((HWND)123);
		workspace1 = workspace1 with { LastFocusedWindowHandle = window.Handle };
		AddActiveWorkspaceToStore(root, workspace1);
		PopulateWindowWorkspaceMap(root, window, workspace1);

		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand(commandName);
		command.TryExecute();

		// Then the window is moved to the next/previous workspace
		Guid expectedWorkspaceId = getNext ? workspace2.Id : workspace3.Id;
		Assert.Contains(
			transforms,
			t => t.Equals(new MoveWindowToWorkspaceTransform(expectedWorkspaceId, window.Handle))
		);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.move_window_to_next_workspace", false, 1)]
	[InlineAutoSubstituteData<StoreCustomization>("whim.core.move_window_to_previous_workspace", true, 2)]
	internal void MoveWindowToAdjacentWorkspace(
		string commandName,
		bool reverse,
		int expectedWorkspaceIndex,
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// Given we have three workspaces
		IWindow window = CreateWindow((HWND)123);

		var workspace1 = CreateWorkspace() with { LastFocusedWindowHandle = window.Handle };
		var workspace2 = CreateWorkspace();
		var workspace3 = CreateWorkspace();
		Workspace[] workspaces = [workspace1, workspace2, workspace3];

		Workspace sourceWorkspace = workspace1;
		Workspace targetWorkspace = workspaces[expectedWorkspaceIndex];

		var monitor = CreateMonitor((HMONITOR)1);

		PopulateMonitorWorkspaceMap(root, monitor, workspace1);

		PopulateWindowWorkspaceMap(root, window, workspace1);
		AddWorkspacesToStore(root, workspace2, workspace3);

		// When we move the window to the next/previous workspace
		CoreCommands commands = new(ctx);
		PluginCommandsTestUtils testUtils = new(commands);
		ICommand command = testUtils.GetCommand(commandName);
		command.TryExecute();

		// Then the window is moved to the expected workspace
		Assert.Contains(transforms, t => t.Equals(new MoveWindowToAdjacentWorkspaceTransform(Reverse: reverse)));
		Assert.Contains(transforms, t => t.Equals(new RemoveWindowFromWorkspaceTransform(sourceWorkspace.Id, window)));
		Assert.Contains(transforms, t => t.Equals(new AddWindowToWorkspaceTransform(targetWorkspace.Id, window)));
	}
}
