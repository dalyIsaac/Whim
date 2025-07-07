using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

internal class CoreCommands : PluginCommands
{
	private readonly IContext _context;

	/// <summary>
	/// The delta for moving a window's edges.
	/// </summary>
	public static int MoveWindowEdgeDelta { get; set; } = 40;

	public CoreCommands(IContext context)
		: base("whim.core")
	{
		_context = context;

		_ = Add(
				identifier: "activate_previous_workspace",
				title: "Activate the previous workspace",
				callback: () => _context.Store.Dispatch(new ActivateAdjacentWorkspaceTransform(Reverse: true)),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "activate_next_workspace",
				title: "Activate the next workspace",
				callback: () => _context.Store.Dispatch(new ActivateAdjacentWorkspaceTransform()),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "focus_window_in_direction.left",
				title: "Focus the window in the left direction",
				callback: () => _context.Store.Dispatch(new FocusWindowInDirectionTransform(Direction: Direction.Left)),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "focus_window_in_direction.right",
				title: "Focus the window in the right direction",
				callback: () =>
					_context.Store.Dispatch(new FocusWindowInDirectionTransform(Direction: Direction.Right)),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "focus_window_in_direction.up",
				title: "Focus the window in the up direction",
				callback: () => _context.Store.Dispatch(new FocusWindowInDirectionTransform(Direction: Direction.Up)),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_UP)
			)
			.Add(
				identifier: "focus_window_in_direction.down",
				title: "Focus the window in the down direction",
				callback: () => _context.Store.Dispatch(new FocusWindowInDirectionTransform(Direction: Direction.Down)),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_DOWN)
			)
			.Add(
				identifier: "swap_window_in_direction.left",
				title: "Swap the window with the window to the left",
				callback: () => _context.Store.Dispatch(new SwapWindowInDirectionTransform(Direction: Direction.Left)),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "swap_window_in_direction.right",
				title: "Swap the window with the window to the right",
				callback: () => _context.Store.Dispatch(new SwapWindowInDirectionTransform(Direction: Direction.Right)),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "swap_window_in_direction.up",
				title: "Swap the window with the window to the up",
				callback: () => _context.Store.Dispatch(new SwapWindowInDirectionTransform(Direction: Direction.Up)),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_UP)
			)
			.Add(
				identifier: "swap_window_in_direction.down",
				title: "Swap the window with the window to the down",
				callback: () => _context.Store.Dispatch(new SwapWindowInDirectionTransform(Direction: Direction.Down)),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_DOWN)
			)
			.Add(
				identifier: "move_window_left_edge_left",
				title: "Move the current window's left edge to the left",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Left,
							new Point<int>() { X = -MoveWindowEdgeDelta, Y = 0 }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_H)
			)
			.Add(
				identifier: "move_window_left_edge_right",
				title: "Move the current window's left edge to the right",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Left,
							new Point<int>() { X = MoveWindowEdgeDelta, Y = 0 }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_J)
			)
			.Add(
				identifier: "move_window_right_edge_left",
				title: "Move the current window's right edge to the left",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Right,
							new Point<int>() { X = -MoveWindowEdgeDelta, Y = 0 }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_K)
			)
			.Add(
				identifier: "move_window_right_edge_right",
				title: "Move the current window's right edge to the right",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Right,
							new Point<int>() { X = MoveWindowEdgeDelta, Y = 0 }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_L)
			)
			.Add(
				identifier: "move_window_top_edge_up",
				title: "Move the current window's top edge up",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Up,
							new Point<int>() { Y = -MoveWindowEdgeDelta }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_U)
			)
			.Add(
				identifier: "move_window_top_edge_down",
				title: "Move the current window's top edge down",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Up,
							new Point<int>() { Y = MoveWindowEdgeDelta }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_I)
			)
			.Add(
				identifier: "move_window_bottom_edge_up",
				title: "Move the current window's bottom edge up",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Down,
							new Point<int>() { Y = -MoveWindowEdgeDelta }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_O)
			)
			.Add(
				identifier: "move_window_bottom_edge_down",
				title: "Move the current window's bottom edge down",
				callback: () =>
					_context.Store.Dispatch(
						new MoveWindowEdgesInDirectionTransform(
							Direction.Down,
							new Point<int>() { Y = MoveWindowEdgeDelta }
						)
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_P)
			)
			.Add(
				identifier: "move_window_to_previous_monitor",
				title: "Move the window to the previous monitor",
				callback: () => _context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(Reverse: true)),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "move_window_to_next_monitor",
				title: "Move the window to the next monitor",
				callback: () => _context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform()),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "move_window_to_next_workspace_on_monitor",
				title: "Move window to next workspace on current monitor",
				callback: MoveWindowToWorkspaceOnCurrentMonitor(getNext: true)
			)
			.Add(
				identifier: "move_window_to_previous_workspace_on_monitor",
				title: "Move window to previous workspace on current monitor",
				callback: MoveWindowToWorkspaceOnCurrentMonitor(getNext: false)
			)
			.Add(
				identifier: "maximize_window",
				title: "Maximize the current window",
				callback: () =>
				{
					if (_context.Store.Pick(PickLastFocusedWindow()).TryGet(out IWindow window))
					{
						window.ShowMaximized();
					}
				}
			)
			.Add(
				identifier: "minimize_window",
				title: "Minimize the current window",
				callback: () =>
				{
					if (_context.Store.Pick(PickLastFocusedWindow()).TryGet(out IWindow window))
					{
						window.ShowMinimized();
					}

					// Find the first non-minimized window and focus it
					foreach (IWindow w in _context.Store.Pick(PickActiveWorkspaceWindows()))
					{
						if (!w.IsMinimized)
						{
							w.Focus();
							break;
						}
					}
				}
			)
			.Add(
				identifier: "cycle_layout_engine.next",
				title: "Cycle to the next layout engine",
				callback: () => _context.Store.Dispatch(new CycleLayoutEngineTransform())
			)
			.Add(
				identifier: "cycle_layout_engine.previous",
				title: "Cycle to the previous layout engine",
				callback: () => _context.Store.Dispatch(new CycleLayoutEngineTransform(Reverse: true))
			)
			.Add(
				identifier: "focus_previous_monitor",
				title: "Focus the previous monitor",
				callback: FocusMonitorInDirection(getNext: false)
			)
			.Add(
				identifier: "focus_next_monitor",
				title: "Focus the next monitor",
				callback: FocusMonitorInDirection(getNext: true)
			)
			.Add(
				identifier: "focus_next_workspace_on_current_monitor",
				title: "Focus the next workspace on the current monitor",
				callback: FocusWorkspaceOnCurrentMonitor(getNext: true)
			)
			.Add(
				identifier: "focus_previous_workspace_on_current_monitor",
				title: "Focus the previous workspace on the current monitor",
				callback: FocusWorkspaceOnCurrentMonitor(getNext: false)
			)
			.Add(
				identifier: "focus_layout.toggle_maximized",
				title: "Toggle the maximized state for the current FocusLayoutEngine",
				callback: () =>
				{
					IWorkspace workspace = _context.Store.Pick(PickActiveWorkspace());
					ILayoutEngine activeLayoutEngine = workspace.GetActiveLayoutEngine();

					if (
						activeLayoutEngine.GetLayoutEngine<FocusLayoutEngine>()
						is not FocusLayoutEngine focusLayoutEngine
					)
					{
						return;
					}

					_context.Store.Dispatch(
						new LayoutEngineCustomActionTransform(
							workspace.Id,
							new() { Name = $"{focusLayoutEngine.Name}.toggle_maximized", Window = null }
						)
					);
				},
				condition: () =>
					_context.Store.Pick(PickActiveLayoutEngine()).GetLayoutEngine<FocusLayoutEngine>() is not null
			)
			.Add(
				identifier: "close_current_workspace",
				title: "Close the current workspace",
				callback: () =>
				{
					WorkspaceId activeWorkspaceId = _context.Store.Pick(PickActiveWorkspaceId());
					_context.Store.Dispatch(new RemoveWorkspaceByIdTransform(activeWorkspaceId));
				},
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_W)
			)
			.Add(identifier: "exit_whim", title: "Exit Whim", callback: () => _context.Exit())
			.Add(
				identifier: "restart_whim",
				title: "Restart Whim",
				callback: () => _context.Exit(new ExitEventArgs() { Reason = ExitReason.Restart })
			);

		for (int idx = 1; idx <= 10; idx++)
		{
			ActivateWorkspaceAtIndex activateWorkspaceAtIndex = new(idx);
			_ = Add(
				identifier: $"activate_workspace_{idx}",
				title: $"Activate workspace {idx}",
				callback: () => activateWorkspaceAtIndex.Execute(context),
				keybind: new Keybind(KeyModifiers.LAlt | KeyModifiers.LShift, key: GetVirtualKeyForInt(idx))
			);
		}

		for (int idx = 1; idx <= 10; idx++)
		{
			MoveActiveWindowToWorkspaceAtIndex moveActiveWindowToWorkspaceAtIndex = new(idx);
			_ = Add(
				identifier: $"move_active_window_to_workspace_{idx}",
				title: $"Move active window to workspace {idx}",
				callback: () => moveActiveWindowToWorkspaceAtIndex.Execute(context)
			);
		}
	}

	internal Action FocusMonitorInDirection(bool getNext) =>
		() =>
		{
			if (!_context.Store.Pick(PickAdjacentMonitor(reverse: !getNext)).TryGet(out IMonitor monitor))
			{
				return;
			}

			if (!_context.Store.Pick(PickWorkspaceByMonitor(monitor.Handle)).TryGet(out IWorkspace workspace))
			{
				return;
			}

			_context.Store.Dispatch(new FocusWorkspaceTransform(workspace.Id));
		};

	internal Action FocusWorkspaceOnCurrentMonitor(bool getNext) =>
		() =>
		{
			IMonitor currentMonitor = _context.Store.Pick(PickActiveMonitor());

			// Get all workspaces that can be shown on this monitor
			if (
				!_context
					.Store.Pick(PickStickyWorkspacesByMonitor(currentMonitor.Handle))
					.TryGet(out IReadOnlyList<IWorkspace> workspaces)
			)
			{
				return;
			}

			// Get current workspace
			WorkspaceId currentWorkspaceId = _context.Store.Pick(PickActiveWorkspaceId());
			int currentIndex = workspaces.FindIndex(w => w.Id == currentWorkspaceId);

			// Calculate direction based on next/previous
			int delta = getNext ? 1 : -1;
			int nextIndex = (currentIndex + delta).Mod(workspaces.Count);

			// Focus the next/previous workspace
			_context.Store.Dispatch(new ActivateWorkspaceTransform(workspaces[nextIndex].Id));
		};

	internal Action MoveWindowToWorkspaceOnCurrentMonitor(bool getNext) =>
		() =>
		{
			if (!_context.Store.Pick(PickLastFocusedWindow()).TryGet(out IWindow window))
			{
				return;
			}

			IMonitor currentMonitor = _context.Store.Pick(PickActiveMonitor());

			// Get all workspaces that can be shown on this monitor
			if (
				!_context
					.Store.Pick(PickStickyWorkspacesByMonitor(currentMonitor.Handle))
					.TryGet(out IReadOnlyList<IWorkspace> workspaces)
			)
			{
				return;
			}

			// Get current workspace
			WorkspaceId currentWorkspaceId = _context.Store.Pick(PickActiveWorkspaceId());
			int currentIndex = workspaces.FindIndex(w => w.Id == currentWorkspaceId);

			// Calculate direction based on next/previous
			int delta = getNext ? 1 : -1;
			int nextIndex = (currentIndex + delta).Mod(workspaces.Count);

			// Move the window to the next/previous workspace
			_context.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspaces[nextIndex].Id, window.Handle));
		};

	// This record is necessary, otherwise the index captured is the last one (11)
	// The index here is 1-based.
	private record ActivateWorkspaceAtIndex(int Index)
	{
		public void Execute(IContext context)
		{
			IWorkspace[] workspaces = [.. context.Store.Pick(PickWorkspaces())];
			if (Index <= workspaces.Length)
			{
				context.Store.Dispatch(new ActivateWorkspaceTransform(workspaces[Index - 1].Id));
			}
		}
	}

	private record MoveActiveWindowToWorkspaceAtIndex(int Index)
	{
		public void Execute(IContext context)
		{
			IWorkspace[] workspaces = [.. context.Store.Pick(PickWorkspaces())];
			if (Index <= workspaces.Length && context.Store.Pick(PickLastFocusedWindow()).TryGet(out IWindow window))
			{
				context.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspaces[Index - 1].Id, window.Handle));
			}
		}
	}

	/// <summary>
	/// Convert the given integer to a <see cref="VIRTUAL_KEY"/>.
	/// This converts the integer 0 - 9 to <see cref="VIRTUAL_KEY.VK_1"/> - <see cref="VIRTUAL_KEY.VK_0"/>.
	/// </summary>
	/// <param name="idx">The integer to convert, 0 - 9.</param>
	/// <returns>The <see cref="VIRTUAL_KEY"/> corresponding to the given integer</returns>
	private static VIRTUAL_KEY GetVirtualKeyForInt(int idx)
	{
		if (idx == 10)
		{
			return VIRTUAL_KEY.VK_0;
		}

		return (VIRTUAL_KEY)((int)VIRTUAL_KEY.VK_1 + (idx - 1));
	}
}
