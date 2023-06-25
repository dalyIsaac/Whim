using System;
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
				callback: () => _context.WorkspaceManager.ActivatePrevious(),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "activate_next_workspace",
				title: "Activate the next workspace",
				callback: () => _context.WorkspaceManager.ActivateNext(),
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
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Left,
						new Point<int>() { X = MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_H)
			)
			.Add(
				identifier: "move_window_left_edge_right",
				title: "Move the current window's left edge to the right",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Left,
						new Point<int>() { X = -MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_J)
			)
			.Add(
				identifier: "move_window_right_edge_left",
				title: "Move the current window's right edge to the left",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Right,
						new Point<int>() { X = -MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_K)
			)
			.Add(
				identifier: "move_window_right_edge_right",
				title: "Move the current window's right edge to the right",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Right,
						new Point<int>() { X = MoveWindowEdgeDelta, Y = 0 }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_L)
			)
			.Add(
				identifier: "move_window_top_edge_up",
				title: "Move the current window's top edge up",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Up,
						new Point<int>() { X = 0, Y = MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_U)
			)
			.Add(
				identifier: "move_window_top_edge_down",
				title: "Move the current window's top edge down",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Up,
						new Point<int>() { X = 0, Y = -MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_I)
			)
			.Add(
				identifier: "move_window_bottom_edge_up",
				title: "Move the current window's bottom edge up",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Down,
						new Point<int>() { X = 0, Y = -MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_O)
			)
			.Add(
				identifier: "move_window_bottom_edge_down",
				title: "Move the current window's bottom edge down",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgesInDirection(
						Direction.Down,
						new Point<int>() { X = 0, Y = MoveWindowEdgeDelta }
					),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_P)
			)
			.Add(
				identifier: "move_window_to_previous_monitor",
				title: "Move the window to the previous monitor",
				callback: () => _context.WorkspaceManager.MoveWindowToPreviousMonitor(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "move_window_to_next_monitor",
				title: "Move the window to the next monitor",
				callback: () => _context.WorkspaceManager.MoveWindowToNextMonitor(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "close_current_workspace",
				title: "Close the current workspace",
				callback: () => _context.WorkspaceManager.Remove(_context.WorkspaceManager.ActiveWorkspace),
				keybind: new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_W)
			)
			.Add(identifier: "exit_whim", title: "Exit Whim", callback: () => _context.Exit());
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

			workspace.FocusWindowInDirection(direction, workspace.LastFocusedWindow);
		};

	/// <summary>
	/// Action to swap the last focused window with the window in the specified direction command.
	/// </summary>
	internal Action SwapWindowInDirection(Direction direction) =>
		() =>
		{
			_context.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction);
		};
}
