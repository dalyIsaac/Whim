using System;
using System.Linq;
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
				callback: () => _context.Butler.ActivateAdjacent(reverse: true),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "activate_next_workspace",
				title: "Activate the next workspace",
				callback: () => _context.Butler.ActivateAdjacent(),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "focus_window_in_direction.left",
				title: "Focus the window in the left direction",
				callback: FocusWindowInDirection(Direction.Left),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "focus_window_in_direction.right",
				title: "Focus the window in the right direction",
				callback: FocusWindowInDirection(Direction.Right),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "focus_window_in_direction.up",
				title: "Focus the window in the up direction",
				callback: FocusWindowInDirection(Direction.Up),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_UP)
			)
			.Add(
				identifier: "focus_window_in_direction.down",
				title: "Focus the window in the down direction",
				callback: FocusWindowInDirection(Direction.Down),
				keybind: new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_DOWN)
			)
			.Add(
				identifier: "swap_window_in_direction.left",
				title: "Swap the window with the window to the left",
				callback: SwapWindowInDirection(Direction.Left),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "swap_window_in_direction.right",
				title: "Swap the window with the window to the right",
				callback: SwapWindowInDirection(Direction.Right),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "swap_window_in_direction.up",
				title: "Swap the window with the window to the up",
				callback: SwapWindowInDirection(Direction.Up),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_UP)
			)
			.Add(
				identifier: "swap_window_in_direction.down",
				title: "Swap the window with the window to the down",
				callback: SwapWindowInDirection(Direction.Down),
				keybind: new Keybind(IKeybind.Win, VIRTUAL_KEY.VK_DOWN)
			)
			.Add(
				identifier: "move_window_left_edge_left",
				title: "Move the current window's left edge to the left",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Left,
						new Point<int>() { X = -MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_H)
			)
			.Add(
				identifier: "move_window_left_edge_right",
				title: "Move the current window's left edge to the right",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Left,
						new Point<int>() { X = MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_J)
			)
			.Add(
				identifier: "move_window_right_edge_left",
				title: "Move the current window's right edge to the left",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Right,
						new Point<int>() { X = -MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_K)
			)
			.Add(
				identifier: "move_window_right_edge_right",
				title: "Move the current window's right edge to the right",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Right,
						new Point<int>() { X = MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_L)
			)
			.Add(
				identifier: "move_window_top_edge_up",
				title: "Move the current window's top edge up",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Up,
						new Point<int>() { Y = -MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_U)
			)
			.Add(
				identifier: "move_window_top_edge_down",
				title: "Move the current window's top edge down",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Up,
						new Point<int>() { Y = MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_I)
			)
			.Add(
				identifier: "move_window_bottom_edge_up",
				title: "Move the current window's bottom edge up",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Down,
						new Point<int>() { Y = -MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_O)
			)
			.Add(
				identifier: "move_window_bottom_edge_down",
				title: "Move the current window's bottom edge down",
				callback: () =>
					_context.Butler.MoveWindowEdgesInDirection(
						Direction.Down,
						new Point<int>() { Y = MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_P)
			)
			.Add(
				identifier: "move_window_to_previous_monitor",
				title: "Move the window to the previous monitor",
				callback: () => _context.Butler.MoveWindowToPreviousMonitor(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "move_window_to_next_monitor",
				title: "Move the window to the next monitor",
				callback: () => _context.Butler.MoveWindowToNextMonitor(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "maximize_window",
				title: "Maximize the current window",
				callback: () => _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow?.ShowMaximized()
			)
			.Add(
				identifier: "minimize_window",
				title: "Minimize the current window",
				callback: () =>
				{
					IWorkspace workspace = _context.WorkspaceManager.ActiveWorkspace;
					workspace.LastFocusedWindow?.ShowMinimized();

					// Find the first non-minimized window and focus it
					foreach (IWindow window in workspace.Windows)
					{
						if (!window.IsMinimized)
						{
							window.Focus();
							break;
						}
					}
				}
			)
			.Add(
				identifier: "cycle_layout_engine.next",
				title: "Cycle to the next layout engine",
				callback: () => _context.WorkspaceManager.ActiveWorkspace.CycleLayoutEngine()
			)
			.Add(
				identifier: "cycle_layout_engine.previous",
				title: "Cycle to the previous layout engine",
				callback: () => _context.WorkspaceManager.ActiveWorkspace.CycleLayoutEngine(reverse: true)
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
				identifier: "focus_layout.toggle_maximized",
				title: "Toggle the maximized state for the current FocusLayoutEngine",
				callback: () =>
				{
					IWorkspace workspace = _context.WorkspaceManager.ActiveWorkspace;
					ILayoutEngine activeLayoutEngine = workspace.ActiveLayoutEngine;

					if (
						activeLayoutEngine.GetLayoutEngine<FocusLayoutEngine>()
						is not FocusLayoutEngine focusLayoutEngine
					)
					{
						return;
					}

					workspace.PerformCustomLayoutEngineAction(
						new LayoutEngineCustomAction()
						{
							Name = $"{focusLayoutEngine.Name}.toggle_maximized",
							Window = null
						}
					);
				},
				condition: () =>
				{
					IWorkspace workspace = _context.WorkspaceManager.ActiveWorkspace;
					ILayoutEngine activeLayoutEngine = workspace.ActiveLayoutEngine;
					return activeLayoutEngine.GetLayoutEngine<FocusLayoutEngine>()
						is FocusLayoutEngine focusLayoutEngine;
				}
			)
			.Add(
				identifier: "close_current_workspace",
				title: "Close the current workspace",
				callback: () => _context.WorkspaceManager.Remove(_context.WorkspaceManager.ActiveWorkspace),
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
	}

	/// <summary>
	/// Action to focus the last focused window in the specified direction command.
	/// </summary>
	internal Action FocusWindowInDirection(Direction direction) =>
		() =>
		{
			IWorkspace workspace = _context.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
			{
				return;
			}

			workspace.FocusWindowInDirection(direction, workspace.LastFocusedWindow, deferLayout: false);
		};

	/// <summary>
	/// Action to swap the last focused window with the window in the specified direction command.
	/// </summary>
	internal Action SwapWindowInDirection(Direction direction) =>
		() => _context.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction);

	internal Action FocusMonitorInDirection(bool getNext) =>
		() =>
		{
			IMonitor active = _context.MonitorManager.ActiveMonitor;
			IMonitor monitor = getNext
				? _context.MonitorManager.GetNextMonitor(active)
				: _context.MonitorManager.GetPreviousMonitor(active);

			IWorkspace? workspace = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor);
			if (workspace == null)
			{
				Logger.Error($"Could not find workspace for monitor {monitor}");
				return;
			}

			workspace.FocusLastFocusedWindow();
		};

	// This record is necessary, otherwise the index captured is the last one (11)
	// The index here is 1-based.
	private record ActivateWorkspaceAtIndex(int Index)
	{
		public void Execute(IContext context)
		{
			IWorkspace[] workspaces = context.WorkspaceManager.ToArray();
			if (Index <= workspaces.Length)
			{
				context.Butler.Activate(workspaces[Index - 1]);
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
