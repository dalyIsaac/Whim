# Commands

Commands (<xref:Whim.ICommand>) are objects with a unique identifier, title, and executable action. They are used to interact with Whim at runtime, for instance, by being bound to [keybinds](./keybinds.md) or the [Command Palette](../plugins/command-palette.md).

Whim differentiates three types of commands.

1. [Core commands](#core-commands) expose common functions as ready-to-use commands, many of which come with a default keybinding.
2. [Plugin commands](#plugin-commands) are ready-to-use commands exposed by plugins.
3. [Custom commands](../../script/core/commands.md) are user-defined commands, which can compose arbitrary functions.

## Core commands

Core commands have identifiers under the `whim.core` namespace.

| Identifier                                               | Title                                                                           | Default Keybind                                      |
| -------------------------------------------------------- | ------------------------------------------------------------------------------- | ---------------------------------------------------- |
| `whim.core.activate_previous_workspace`                  | Activate the previous workspace                                                 | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>LEFT</kbd>   |
| `whim.core.activate_next_workspace`                      | Activate the next workspace                                                     | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>RIGHT</kbd>  |
| `whim.core.focus_window_in_direction.left`               | Focus the window in the left direction                                          | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>LEFT</kbd>    |
| `whim.core.focus_window_in_direction.right`              | Focus the window in the right direction                                         | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>RIGHT</kbd>   |
| `whim.core.focus_window_in_direction.up`                 | Focus the window in the up direction                                            | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>UP</kbd>      |
| `whim.core.focus_window_in_direction.down`               | Focus the window in the down direction                                          | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>DOWN</kbd>    |
| `whim.core.swap_window_in_direction.left`                | Swap the window with the window to the left                                     | <kbd>Win</kbd> + <kbd>LEFT</kbd>                     |
| `whim.core.swap_window_in_direction.right`               | Swap the window with the window to the right                                    | <kbd>Win</kbd> + <kbd>RIGHT</kbd>                    |
| `whim.core.swap_window_in_direction.up`                  | Swap the window with the window to the up                                       | <kbd>Win</kbd> + <kbd>UP</kbd>                       |
| `whim.core.swap_window_in_direction.down`                | Swap the window with the window to the down                                     | <kbd>Win</kbd> + <kbd>DOWN</kbd>                     |
| `whim.core.move_window_left_edge_left`                   | Move the current window's left edge to the left                                 | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>H</kbd>      |
| `whim.core.move_window_left_edge_right`                  | Move the current window's left edge to the right                                | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>J</kbd>      |
| `whim.core.move_window_right_edge_left`                  | Move the current window's right edge to the left                                | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>K</kbd>      |
| `whim.core.move_window_right_edge_right`                 | Move the current window's right edge to the right                               | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>L</kbd>      |
| `whim.core.move_window_top_edge_up`                      | Move the current window's top edge up                                           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>U</kbd>      |
| `whim.core.move_window_top_edge_down`                    | Move the current window's top edge down                                         | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>I</kbd>      |
| `whim.core.move_window_bottom_edge_up`                   | Move the current window's bottom edge up                                        | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>O</kbd>      |
| `whim.core.move_window_bottom_edge_down`                 | Move the current window's bottom edge down                                      | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>P</kbd>      |
| `whim.core.move_window_to_previous_monitor`              | Move the window to the previous monitor                                         | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.core.move_window_to_next_monitor`                  | Move the window to the next monitor                                             | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.core.move_window_to_next_workspace_on_monitor`     | Move window to next workspace on current monitor\*                              | No default keybind                                   |
| `whim.core.move_window_to_previous_workspace_on_monitor` | Move window to previous workspace on current monitor\*                          | No default keybind                                   |
| `whim.core.maximize_window`                              | Maximize the current window                                                     | No default keybind                                   |
| `whim.core.minimize_window`                              | Minimize the current window                                                     | No default keybind                                   |
| `whim.core.cycle_layout_engine.next`                     | Cycle to the next layout engine                                                 | No default keybind                                   |
| `whim.core.cycle_layout_engine.previous`                 | Cycle to the previous layout engine                                             | No default keybind                                   |
| `whim.core.focus_previous_monitor`                       | Focus the previous monitor                                                      | No default keybind                                   |
| `whim.core.focus_next_monitor`                           | Focus the next monitor                                                          | No default keybind                                   |
| `whim.core.focus_next_workspace_on_current_monitor`      | Focus the next workspace on the current monitor\*                               | No default keybind                                   |
| `whim.core.focus_previous_workspace_on_current_monitor`  | Focus the previous workspace on the current monitor\*                           | No default keybind                                   |
| `whim.core.focus_layout.toggle_maximized`                | Toggle the maximized state for the current FocusLayoutEngine                    | No default keybind                                   |
| `whim.core.close_current_workspace`                      | Close the current workspace                                                     | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>W</kbd>      |
| `whim.core.exit_whim`                                    | Exit Whim                                                                       | No default keybind                                   |
| `whim.core.restart_whim`                                 | Restart Whim                                                                    | No default keybind                                   |
| `whim.core.activate_workspace_{idx}`                     | Activate workspace `{idx}` (where `idx` is an `int` 1, 2, ...9, 0)              | <kbd>Alt</kbd> + <kbd>Shift</kbd> + <kbd>{idx}</kbd> |
| `whim.core.move_active_window_to_workspace_{idx}`        | Move active window to workspace `{idx}` (where `idx` is an `int` 1, 2, ...9, 0) | No default keybind                                   |

\* These commands account for [sticky workspaces](workspaces.md#sticky-workspaces) when determining the next/previous workspace.

## Plugin commands

Plugin commands are namespaced by the string defined in the <xref:Whim.IPlugin.Name> property for plugins - for example, `whim.gaps` for [`GapsPlugin`](../plugins/gaps.md). Commands are listed on the respective plugin documentation pages.
