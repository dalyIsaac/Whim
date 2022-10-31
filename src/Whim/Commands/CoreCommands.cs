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

	private readonly IConfigContext _configContext;

	/// <summary>
	/// Creates a new instance of the core commands command.
	/// </summary>
	public CoreCommands(IConfigContext configContext)
	{
		_configContext = configContext;
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
		yield return MoveWindowToPreviousMonitor;
		yield return MoveWindowToNextMonitor;
		yield return CloseCurrentWorkspace;
		yield return ExitWhim;
		yield return ExitWhim;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

	/// <summary>
	/// Action to focus the last focused window in the specified direction command.
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
	/// Action to swap the last focused window with the window in the specified direction command.
	/// </summary>
	public static Action SwapWindowInDirection(IConfigContext configContext, Direction direction) => () =>
	{
		configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction);
	};

	/// <summary>
	/// Focus the window in the left direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionLeft => new(
		new Command(
			identifier: "${Name}.focus_window_in_direction.left",
			title: "Focus the window in the left direction",
			callback: FocusWindowInDirection(_configContext, Direction.Left)
		),
		new Keybind(WinAlt, VIRTUAL_KEY.VK_LEFT)
	);

	/// <summary>
	/// Focus the window in the right direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionRight => new(
		new Command(
			identifier: "${Name}.focus_window_in_direction.right",
			title: "Focus the window in the right direction",
			callback: FocusWindowInDirection(_configContext, Direction.Right)
		),
		new Keybind(WinAlt, VIRTUAL_KEY.VK_RIGHT)
	);

	/// <summary>
	/// Focus the window in the up direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionUp => new(
		new Command(
			identifier: "${Name}.focus_window_in_direction.up",
			title: "Focus the window in the up direction",
			callback: FocusWindowInDirection(_configContext, Direction.Up)
		),
		new Keybind(WinAlt, VIRTUAL_KEY.VK_UP)
	);

	/// <summary>
	/// Focus the window in the down direction command.
	/// </summary>
	public CommandItem FocusWindowInDirectionDown => new(
		new Command(
			identifier: "${Name}.focus_window_in_direction.down",
			title: "Focus the window in the down direction",
			callback: FocusWindowInDirection(_configContext, Direction.Down)
		),
		new Keybind(WinAlt, VIRTUAL_KEY.VK_DOWN)
	);

	/// <summary>
	/// Swap the window with the window in the left direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionLeft => new(
		new Command(
			identifier: "${Name}.swap_window_in_direction.left",
			title: "Swap the window with the window to the left",
			callback: SwapWindowInDirection(_configContext, Direction.Left)
		),
		new Keybind(WinCtrl, VIRTUAL_KEY.VK_LEFT)
	);

	/// <summary>
	/// Swap the window with the window in the right direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionRight => new(
		new Command(
			identifier: "${Name}.swap_window_in_direction.right",
			title: "Swap the window with the window to the right",
			callback: SwapWindowInDirection(_configContext, Direction.Right)
		),
		new Keybind(WinCtrl, VIRTUAL_KEY.VK_RIGHT)
	);

	/// <summary>
	/// Swap the window with the window in the up direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionUp => new(
		new Command(
			identifier: "${Name}.swap_window_in_direction.up",
			title: "Swap the window with the window to the up",
			callback: SwapWindowInDirection(_configContext, Direction.Up)
		),
		new Keybind(WinCtrl, VIRTUAL_KEY.VK_UP)
	);

	/// <summary>
	/// Swap the window with the window in the down direction command.
	/// </summary>
	public CommandItem SwapWindowInDirectionDown => new(
		new Command(
			identifier: "${Name}.swap_window_in_direction.down",
			title: "Swap the window with the window to the down",
			callback: SwapWindowInDirection(_configContext, Direction.Down)
		),
		new Keybind(WinCtrl, VIRTUAL_KEY.VK_DOWN)
	);

	/// <summary>
	/// Move window to the previous monitor command.
	/// </summary>
	public CommandItem MoveWindowToPreviousMonitor => new(
		new Command(
			identifier: "${Name}.move_window_to_previous_monitor",
			title: "Move the window to the previous monitor",
			callback: () => _configContext.WorkspaceManager.MoveWindowToPreviousMonitor()
		),
		new Keybind(WinShift, VIRTUAL_KEY.VK_LEFT)
	);

	/// <summary>
	/// Move window to the next monitor command.
	/// </summary>
	public CommandItem MoveWindowToNextMonitor => new(
		new Command(
			identifier: "${Name}.move_window_to_next_monitor",
			title: "Move the window to the next monitor",
			callback: () => _configContext.WorkspaceManager.MoveWindowToNextMonitor()
		),
		new Keybind(WinShift, VIRTUAL_KEY.VK_RIGHT)
	);

	/// <summary>
	/// Close the current workspace command.
	/// </summary>
	public CommandItem CloseCurrentWorkspace => new(
		new Command(
			identifier: "${Name}.close_current_workspace",
			title: "Close the current workspace",
			callback: () => _configContext.WorkspaceManager.Remove(_configContext.WorkspaceManager.ActiveWorkspace)
		),
		new Keybind(WinCtrl, VIRTUAL_KEY.VK_W)
	);

	/// <summary>
	/// Exit Whim command.
	/// </summary>
	public CommandItem ExitWhim => new(
		new Command(
			identifier: "${Name}.exit_whim",
			title: "Exit Whim",
			callback: () => _configContext.Exit()
		),
		null
	);
}
