using System;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
/// Whim's default commands.
/// </summary>
public static class DefaultCommands
{
	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Alt</c>.
	/// </summary>
	public const KeyModifiers WinAlt = KeyModifiers.LWin | KeyModifiers.LAlt;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Shift</c>.
	/// </summary>
	public const KeyModifiers WinShift = KeyModifiers.LWin | KeyModifiers.LShift;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c>.
	/// </summary>
	public const KeyModifiers WinCtrl = KeyModifiers.LWin | KeyModifiers.LControl;

	/// <summary>
	/// The value for the key modifier <c>Win</c> + <c>Ctrl</c> + <c>Shift</c>.
	/// </summary>
	public const KeyModifiers WinCtrlShift = KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift;

	/// <summary>
	/// Gets the default commands and their associated keybinds.
	/// </summary>
	public static CommandItem[] GetCommands(IConfigContext configContext) => new CommandItem[]
	{
		// Focus window in direction.
		new CommandItem(
			new Command(
				identifier: "default_commands.focus_window_in_direction.left",
				title: "Focus the window in the left direction",
				callback: FocusWindowInDirection(configContext, Direction.Left)
			),
			new Keybind(WinAlt, VIRTUAL_KEY.VK_LEFT)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.focus_window_in_direction.right",
				title: "Focus the window in the right direction",
				callback: FocusWindowInDirection(configContext, Direction.Right)
			),
			new Keybind(WinAlt, VIRTUAL_KEY.VK_RIGHT)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.focus_window_in_direction.up",
				title: "Focus the window in the up direction",
				callback: FocusWindowInDirection(configContext, Direction.Up)
			),
			new Keybind(WinAlt, VIRTUAL_KEY.VK_UP)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.focus_window_in_direction.down",
				title: "Focus the window in the down direction",
				callback: FocusWindowInDirection(configContext, Direction.Down)
			),
			new Keybind(WinAlt, VIRTUAL_KEY.VK_DOWN)
		),

		// Swap windows in direction.
		new CommandItem(
			new Command(
				identifier: "default_commands.swap_window_in_direction.left",
				title: "Swap the window with the window to the left",
				callback: SwapWindowInDirection(configContext, Direction.Left)
			),
			new Keybind(WinCtrl, VIRTUAL_KEY.VK_LEFT)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.swap_window_in_direction.right",
				title: "Swap the window with the window to the right",
				callback: SwapWindowInDirection(configContext, Direction.Right)
			),
			new Keybind(WinCtrl, VIRTUAL_KEY.VK_RIGHT)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.swap_window_in_direction.up",
				title: "Swap the window with the window to the up",
				callback: SwapWindowInDirection(configContext, Direction.Up)
			),
			new Keybind(WinCtrl, VIRTUAL_KEY.VK_UP)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.swap_window_in_direction.down",
				title: "Swap the window with the window to the down",
				callback: SwapWindowInDirection(configContext, Direction.Down)
			),
			new Keybind(WinCtrl, VIRTUAL_KEY.VK_DOWN)
		),

		// Move window to monitor.
		new CommandItem(
			new Command(
				identifier: "default_commands.move_window_to_monitor.previous",
				title: "Move the window to the previous monitor",
				callback: () => configContext.WorkspaceManager.MoveWindowToPreviousMonitor()
			),
			new Keybind(WinShift, VIRTUAL_KEY.VK_LEFT)
		),
		new CommandItem(
			new Command(
				identifier: "default_commands.move_window_to_monitor.next",
				title: "Move the window to the next monitor",
				callback: () => configContext.WorkspaceManager.MoveWindowToNextMonitor()
			),
			new Keybind(WinShift, VIRTUAL_KEY.VK_RIGHT)
		),

		// Exit.
		new CommandItem(
			new Command(
				identifier: "default_commands.exit",
				title: "Exit Whim",
				callback: () => configContext.Exit()
			),
			null
		),
	};

	/// <summary>
	/// Action to focus the last focused window in the specified direction.
	/// </summary>
	public static Action FocusWindowInDirection(IConfigContext configContext, Direction direction) => () =>
	{
		IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
		if (workspace.LastFocusedWindow == null)
		{
			return;
		}

		workspace.ActiveLayoutEngine.FocusWindowInDirection(direction, workspace.LastFocusedWindow);
	};

	/// <summary>
	/// Action to swap the last focused window with the window in the specified direction.
	/// </summary>
	public static Action SwapWindowInDirection(IConfigContext configContext, Direction direction) => () =>
	{
		configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction);
	};
}
