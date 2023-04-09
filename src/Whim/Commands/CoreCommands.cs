using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// Whim's default commands.
/// </summary>
public class CoreCommands : IEnumerable<CommandItem>
{
	/// <summary>
	/// The name prefix for all the core commands command.
	/// </summary>
	public const string Name = "whim.core";

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Alt</c> command.
	/// </summary>
	public const KeyModifiers WinAlt = KeyModifiers.LWin | KeyModifiers.LAlt;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Shift</c> command.
	/// </summary>
	public const KeyModifiers WinShift = KeyModifiers.LWin | KeyModifiers.LShift;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> command.
	/// </summary>
	public const KeyModifiers WinCtrl = KeyModifiers.LWin | KeyModifiers.LControl;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> + <c>Shift</c> command.
	/// </summary>
	public const KeyModifiers WinCtrlShift = KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift;

	/// <summary>
	/// The delta for moving a window's edges.
	/// </summary>
	public static double MoveWindowEdgeDelta { get; set; } = 0.05;

	private readonly IContext _context;

	/// <summary>
	/// Creates a new instance of the core commands command.
	/// </summary>
	public CoreCommands(IContext context)
	{
		_context = context;
	}

	/// <inheritdoc/>
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return FocusWindowInDirectionLeft;
		yield return FocusWindowInDirectionRight;
		yield return FocusWindowInDirectionUp;
		yield return FocusWindowInDirectionDown;

		yield return SwapWindowInDirectionLeft;
		yield return SwapWindowInDirectionRight;
		yield return SwapWindowInDirectionUp;
		yield return SwapWindowInDirectionDown;

		yield return MoveWindowLeftEdgeLeft;
		yield return MoveWindowLeftEdgeRight;
		yield return MoveWindowRightEdgeLeft;
		yield return MoveWindowRightEdgeRight;
		yield return MoveWindowTopEdgeUp;
		yield return MoveWindowTopEdgeDown;
		yield return MoveWindowBottomEdgeUp;
		yield return MoveWindowBottomEdgeDown;

		yield return MoveWindowToPreviousMonitor;
		yield return MoveWindowToNextMonitor;

		yield return CloseCurrentWorkspace;
		yield return ExitWhim;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Action to focus the last focused window in the specified direction command.
	/// </summary>
	public static Action FocusWindowInDirection(IContext context, Direction direction) =>
		() =>
		{
			IWorkspace workspace = context.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
			{
				return;
			}

			workspace.FocusWindowInDirection(direction, workspace.LastFocusedWindow);
		};

	/// <summary>
	/// Action to swap the last focused window with the window in the specified direction command.
	/// </summary>
	public static Action SwapWindowInDirection(IContext context, Direction direction) =>
		() =>
		{
			context.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction);
		};

	/// <summary>
	/// Focus the window in the left direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionLeft =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.focus_window_in_direction.left",
				title: "Focus the window in the left direction",
				callback: FocusWindowInDirection(_context, Direction.Left)
			),
			Keybind = new Keybind(WinAlt, VIRTUAL_KEY.VK_LEFT)
		};

	/// <summary>
	/// Focus the window in the right direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionRight =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.focus_window_in_direction.right",
				title: "Focus the window in the right direction",
				callback: FocusWindowInDirection(_context, Direction.Right)
			),
			Keybind = new Keybind(WinAlt, VIRTUAL_KEY.VK_RIGHT)
		};

	/// <summary>
	/// Focus the window in the up direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionUp =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.focus_window_in_direction.up",
				title: "Focus the window in the up direction",
				callback: FocusWindowInDirection(_context, Direction.Up)
			),
			Keybind = new Keybind(WinAlt, VIRTUAL_KEY.VK_UP)
		};

	/// <summary>
	/// Focus the window in the down direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionDown =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.focus_window_in_direction.down",
				title: "Focus the window in the down direction",
				callback: FocusWindowInDirection(_context, Direction.Down)
			),
			Keybind = new Keybind(WinAlt, VIRTUAL_KEY.VK_DOWN)
		};

	/// <summary>
	/// Swap the window with the window in the left direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionLeft =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.swap_window_in_direction.left",
				title: "Swap the window with the window to the left",
				callback: SwapWindowInDirection(_context, Direction.Left)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_LEFT)
		};

	/// <summary>
	/// Swap the window with the window in the right direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionRight =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.swap_window_in_direction.right",
				title: "Swap the window with the window to the right",
				callback: SwapWindowInDirection(_context, Direction.Right)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_RIGHT)
		};

	/// <summary>
	/// Swap the window with the window in the up direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionUp =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.swap_window_in_direction.up",
				title: "Swap the window with the window to the up",
				callback: SwapWindowInDirection(_context, Direction.Up)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_UP)
		};

	/// <summary>
	/// Swap the window with the window in the down direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionDown =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.swap_window_in_direction.down",
				title: "Swap the window with the window to the down",
				callback: SwapWindowInDirection(_context, Direction.Down)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_DOWN)
		};

	/// <summary>
	/// Move the current window's left edge to the left.
	/// </summary>
	public CommandItem MoveWindowLeftEdgeLeft =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_left_edge_left",
				title: "Move the current window's left edge to the left",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Left,
						MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_H)
		};

	/// <summary>
	/// Move the current window's left edge to the right.
	/// </summary>
	public CommandItem MoveWindowLeftEdgeRight =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_left_edge_right",
				title: "Move the current window's left edge to the right",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Left,
						-MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_J)
		};

	/// <summary>
	/// Move the current window's right edge to the left.
	/// </summary>
	public CommandItem MoveWindowRightEdgeLeft =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_right_edge_left",
				title: "Move the current window's right edge to the left",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Right,
						-MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_K)
		};

	/// <summary>
	/// Move the current window's right edge to the right.
	/// </summary>
	public CommandItem MoveWindowRightEdgeRight =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_right_edge_right",
				title: "Move the current window's right edge to the right",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Right,
						MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_L)
		};

	/// <summary>
	/// Move the current window's top edge up.
	/// </summary>
	public CommandItem MoveWindowTopEdgeUp =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_top_edge_up",
				title: "Move the current window's top edge up",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Up,
						MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_U)
		};

	/// <summary>
	/// Move the current window's top edge down.
	/// </summary>
	public CommandItem MoveWindowTopEdgeDown =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_top_edge_down",
				title: "Move the current window's top edge down",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Up,
						-MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_I)
		};

	/// <summary>
	/// Move the current window's bottom edge up.
	/// </summary>
	public CommandItem MoveWindowBottomEdgeUp =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_bottom_edge_up",
				title: "Move the current window's bottom edge up",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Down,
						-MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_O)
		};

	/// <summary>
	/// Move the current window's bottom edge down.
	/// </summary>
	public CommandItem MoveWindowBottomEdgeDown =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_bottom_edge_down",
				title: "Move the current window's bottom edge down",
				callback: () =>
					_context.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(
						Direction.Down,
						MoveWindowEdgeDelta
					)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_P)
		};

	/// <summary>
	/// Move window to the previous monitor command.
	/// </summary>
	public CommandItem MoveWindowToPreviousMonitor =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_to_previous_monitor",
				title: "Move the window to the previous monitor",
				callback: () => _context.WorkspaceManager.MoveWindowToPreviousMonitor()
			),
			Keybind = new Keybind(WinShift, VIRTUAL_KEY.VK_LEFT)
		};

	/// <summary>
	/// Move window to the next monitor command.
	/// </summary>
	public CommandItem MoveWindowToNextMonitor =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_to_next_monitor",
				title: "Move the window to the next monitor",
				callback: () => _context.WorkspaceManager.MoveWindowToNextMonitor()
			),
			Keybind = new Keybind(WinShift, VIRTUAL_KEY.VK_RIGHT)
		};

	/// <summary>
	/// Close the current workspace command.
	/// </summary>
	public CommandItem CloseCurrentWorkspace =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.close_current_workspace",
				title: "Close the current workspace",
				callback: () => _context.WorkspaceManager.Remove(_context.WorkspaceManager.ActiveWorkspace)
			),
			Keybind = new Keybind(WinCtrl, VIRTUAL_KEY.VK_W)
		};

	/// <summary>
	/// Exit Whim command.
	/// </summary>
	public CommandItem ExitWhim =>
		new()
		{
			Command = new Command(identifier: $"{Name}.exit_whim", title: "Exit Whim", callback: () => _context.Exit()),
			Keybind = null
		};
}
