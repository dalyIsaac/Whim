# Whim

Whim is a pluggable and modern window manager for Windows 10 and 11, built using WinUI 3 and .NET. It is currently in active development.

![Whim demo](docs/images/demo.gif)

#### Default Keybindings

Keybindings can also be seen in the command palette, when it is activated (<kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> by default).

##### Core Commands

These are the commands and associated keybindings provided by Whim's core. See [`CoreCommands.cs`](src/Whim/Commands/CoreCommands.cs).

| Identifier                                  | Title                                                            | Keybind                                              |
| ------------------------------------------- | ---------------------------------------------------------------- | ---------------------------------------------------- |
| `whim.core.activate_previous_workspace`     | Activate the previous workspace                                  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>LEFT</kbd>   |
| `whim.core.activate_next_workspace`         | Activate the next workspace                                      | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>RIGHT</kbd>  |
| `whim.core.focus_window_in_direction.left`  | Focus the window in the left direction                           | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>LEFT</kbd>    |
| `whim.core.focus_window_in_direction.right` | Focus the window in the right direction                          | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>RIGHT</kbd>   |
| `whim.core.focus_window_in_direction.up`    | Focus the window in the up direction                             | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>UP</kbd>      |
| `whim.core.focus_window_in_direction.down`  | Focus the window in the down direction                           | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>DOWN</kbd>    |
| `whim.core.swap_window_in_direction.left`   | Swap the window with the window to the left                      | <kbd>Win</kbd> + <kbd>LEFT</kbd>                     |
| `whim.core.swap_window_in_direction.right`  | Swap the window with the window to the right                     | <kbd>Win</kbd> + <kbd>RIGHT</kbd>                    |
| `whim.core.swap_window_in_direction.up`     | Swap the window with the window to the up                        | <kbd>Win</kbd> + <kbd>UP</kbd>                       |
| `whim.core.swap_window_in_direction.down`   | Swap the window with the window to the down                      | <kbd>Win</kbd> + <kbd>DOWN</kbd>                     |
| `whim.core.move_window_left_edge_left`      | Move the current window's left edge to the left                  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>H</kbd>      |
| `whim.core.move_window_left_edge_right`     | Move the current window's left edge to the right                 | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>J</kbd>      |
| `whim.core.move_window_right_edge_left`     | Move the current window's right edge to the left                 | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>K</kbd>      |
| `whim.core.move_window_right_edge_right`    | Move the current window's right edge to the right                | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>L</kbd>      |
| `whim.core.move_window_top_edge_up`         | Move the current window's top edge up                            | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>U</kbd>      |
| `whim.core.move_window_top_edge_down`       | Move the current window's top edge down                          | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>I</kbd>      |
| `whim.core.move_window_bottom_edge_up`      | Move the current window's bottom edge up                         | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>O</kbd>      |
| `whim.core.move_window_bottom_edge_down`    | Move the current window's bottom edge down                       | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>P</kbd>      |
| `whim.core.move_window_to_previous_monitor` | Move the window to the previous monitor                          | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.core.move_window_to_next_monitor`     | Move the window to the next monitor                              | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.core.focus_previous_monitor`          | Focus the previous monitor                                       | No default keybind                                   |
| `whim.core.focus_next_monitor`              | Focus the next monitor                                           | No default keybind                                   |
| `whim.core.close_current_workspace`         | Close the current workspace                                      | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>W</kbd>      |
| `whim.core.exit_whim`                       | Exit Whim                                                        | No default keybind                                   |
| `whim.core.activate_workspace_{idx}`        | Activate workspace `{idx}` (where `idx` is an int 1, 2, ...9, 0) | <kbd>Alt</kbd> + <kbd>Shift</kbd> + <kbd>{idx}</kbd> |

##### Command Palette Plugin Commands

See [`CommandPaletteCommands.cs`](src/Whim.CommandPalette/CommandPaletteCommands.cs).

| Identifier                                                | Title                              | Keybind                                          |
| --------------------------------------------------------- | ---------------------------------- | ------------------------------------------------ |
| `whim.command_palette.toggle`                             | Toggle command palette             | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> |
| `whim.command_palette.activate_workspace`                 | Activate workspace                 | No default keybind                               |
| `whim.command_palette.rename_workspace`                   | Rename workspace                   | No default keybind                               |
| `whim.command_palette.create_workspace`                   | Create workspace                   | No default keybind                               |
| `whim.command_palette.move_window_to_workspace`           | Move window to workspace           | No default keybind                               |
| `whim.command_palette.move_multiple_windows_to_workspace` | Move multiple windows to workspace | No default keybind                               |
| `whim.command_palette.remove_window`                      | Select window to remove from Whim  | No default keybind                               |

##### Floating Layout Plugin Commands

See [`FloatingLayoutCommands.cs`](src/Whim.FloatingLayout/FloatingLayoutCommands.cs).

| Identifier                                     | Title                   | Keybind                                          |
| ---------------------------------------------- | ----------------------- | ------------------------------------------------ |
| `whim.floating_layout.toggle_window_floating`  | Toggle window floating  | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>F</kbd> |
| `whim.floating_layout.mark_window_as_floating` | Mark window as floating | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>M</kbd> |
| `whim.floating_layout.mark_window_as_docked`   | Mark window as docked   | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>D</kbd> |

##### Focus Indicator Plugin Commands

See [`FocusIndicatorCommands.cs`](src/Whim.FocusIndicator/FocusIndicatorCommands.cs).

| Identifier                            | Title                                         | Keybind            |
| ------------------------------------- | --------------------------------------------- | ------------------ |
| `whim.focus_indicator.show`           | Show focus indicator                          | No default keybind |
| `whim.focus_indicator.toggle`         | Toggle focus indicator                        | No default keybind |
| `whim.focus_indicator.toggle_fade`    | Toggle whether the focus indicator fades      | No default keybind |
| `whim.focus_indicator.toggle_enabled` | Toggle whether the focus indicator is enabled | No default keybind |

##### Slice Layout Plugin Commands

See [`SliceLayoutCommands.cs`](src/Whim.SliceLayout/SliceLayoutCommands.cs).

| Identifier                                    | Title                              | Keybind            |
| --------------------------------------------- | ---------------------------------- | ------------------ |
| `whim.slice_layout.set_insertion_type.swap`   | Set slice insertion type to swap   | No default keybind |
| `whim.slice_layout.set_insertion_type.rotate` | Set slice insertion type to rotate | No default keybind |
| `whim.slice_layout.window.promote`            | Promote window in stack            | No default keybind |
| `whim.slice_layout.window.demote`             | Demote window in stack             | No default keybind |
| `whim.slice_layout.focus.promote`             | Promote focus in stack             | No default keybind |
| `whim.slice_layout.focus.demote`              | Demote focus in stack              | No default keybind |

##### Tree Layout Plugin Commands

See [`TreeLayoutCommands.cs`](src/Whim.TreeLayout/TreeLayoutCommands.cs).

| Identifier                                  | Title                                          | Keybind                                                                |
| ------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------- |
| `whim.tree_layout.add_tree_direction_left`  | Add windows to the left of the current window  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.tree_layout.add_tree_direction_right` | Add windows to the right of the current window | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.tree_layout.add_tree_direction_up`    | Add windows above the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>UP</kbd>    |
| `whim.tree_layout.add_tree_direction_down`  | Add windows below the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>DOWN</kbd>  |

## Automatic Updating

The `Whim.Updater` plugin is in `alpha` (especially as Whim hasn't started releasing non-`alpha` builds). If the updater fails, you can manually update Whim by downloading the latest release from the [releases page](https://github.com/dalyIsaac/Whim/releases).

The updater will show a notification when a new version is available. Clicking on the notification will show the changelog for the delta between the current version and the latest version.

The `UpdaterConfig` supports specifying the `ReleaseChannel` and `UpdateFrequency`.
