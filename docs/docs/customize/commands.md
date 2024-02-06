# Commands

Commands are used to interact with Whim at runtime. They can be used as [keybinds](./keybinds.md) or triggered from the [Command Palette](../plugins/command-palette.md) (activated by <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> by default).

Whim supports the creation of arbitrary commands using the <xref:Whim.ICommandManager> - see [Custom commands](#custom-commands).

## Core commands

| Identifier                                  | Title                                                              | Keybind                                              |
| ------------------------------------------- | ------------------------------------------------------------------ | ---------------------------------------------------- |
| `whim.core.activate_previous_workspace`     | Activate the previous workspace                                    | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>LEFT</kbd>   |
| `whim.core.activate_next_workspace`         | Activate the next workspace                                        | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>RIGHT</kbd>  |
| `whim.core.focus_window_in_direction.left`  | Focus the window in the left direction                             | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>LEFT</kbd>    |
| `whim.core.focus_window_in_direction.right` | Focus the window in the right direction                            | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>RIGHT</kbd>   |
| `whim.core.focus_window_in_direction.up`    | Focus the window in the up direction                               | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>UP</kbd>      |
| `whim.core.focus_window_in_direction.down`  | Focus the window in the down direction                             | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>DOWN</kbd>    |
| `whim.core.swap_window_in_direction.left`   | Swap the window with the window to the left                        | <kbd>Win</kbd> + <kbd>LEFT</kbd>                     |
| `whim.core.swap_window_in_direction.right`  | Swap the window with the window to the right                       | <kbd>Win</kbd> + <kbd>RIGHT</kbd>                    |
| `whim.core.swap_window_in_direction.up`     | Swap the window with the window to the up                          | <kbd>Win</kbd> + <kbd>UP</kbd>                       |
| `whim.core.swap_window_in_direction.down`   | Swap the window with the window to the down                        | <kbd>Win</kbd> + <kbd>DOWN</kbd>                     |
| `whim.core.move_window_left_edge_left`      | Move the current window's left edge to the left                    | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>H</kbd>      |
| `whim.core.move_window_left_edge_right`     | Move the current window's left edge to the right                   | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>J</kbd>      |
| `whim.core.move_window_right_edge_left`     | Move the current window's right edge to the left                   | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>K</kbd>      |
| `whim.core.move_window_right_edge_right`    | Move the current window's right edge to the right                  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>L</kbd>      |
| `whim.core.move_window_top_edge_up`         | Move the current window's top edge up                              | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>U</kbd>      |
| `whim.core.move_window_top_edge_down`       | Move the current window's top edge down                            | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>I</kbd>      |
| `whim.core.move_window_bottom_edge_up`      | Move the current window's bottom edge up                           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>O</kbd>      |
| `whim.core.move_window_bottom_edge_down`    | Move the current window's bottom edge down                         | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>P</kbd>      |
| `whim.core.move_window_to_previous_monitor` | Move the window to the previous monitor                            | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.core.move_window_to_next_monitor`     | Move the window to the next monitor                                | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.core.cycle_layout_engine.next`        | Cycle to the next layout engine                                    | No default keybind                                   |
| `whim.core.cycle_layout_engine.previous`    | Cycle to the previous layout engine                                | No default keybind                                   |
| `whim.core.focus_previous_monitor`          | Focus the previous monitor                                         | No default keybind                                   |
| `whim.core.focus_next_monitor`              | Focus the next monitor                                             | No default keybind                                   |
| `whim.core.close_current_workspace`         | Close the current workspace                                        | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>W</kbd>      |
| `whim.core.exit_whim`                       | Exit Whim                                                          | No default keybind                                   |
| `whim.core.activate_workspace_{idx}`        | Activate workspace `{idx}` (where `idx` is an `int` 1, 2, ...9, 0) | <kbd>Alt</kbd> + <kbd>Shift</kbd> + <kbd>{idx}</kbd> |

## Advanced usage

### Namespaces

Whim stores commands (<xref:Whim.ICommand>) in the <xref:Whim.ICommandManager>. Commands are objects with a unique identifier, title, and executable action.

Command identifiers are namespaced:

- `whim.core` is reserved for [core commands](#core-commands)
- the string defined in the <xref:Whim.IPlugin.Name> property for plugins - for example, `whim.gaps` for [`GapsPlugin`](../plugins/gaps.md)
- `whim.custom` is reserved for [custom user-defined commands](#custom-commands)

### Custom commands

Custom commands are automatically added to the `whim.custom` namespace. For example, the following command closes the current tracked window:

```csharp
// Create the command.
context.CommandManager.Add(
    // Automatically namespaced to `whim.custom`.
    identifier: "close_window",
    title: "Close focused window",
    callback: () => context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Close()
);

// Create an associated keybind.
context.KeybindManager.SetKeybind("whim.custom.close_window", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_D));
```
