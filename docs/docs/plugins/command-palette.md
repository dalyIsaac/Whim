# Command Palette Plugin

The <xref:Whim.CommandPalette.CommandPalettePlugin> provides a fuzzy command palette, loaded with all the <xref:Whim.ICommand>s registered with the <xref:Whim.ICommandManager>.

The command palette uses a port of Visual Studio Code's fuzzy search algorithm.

![Command palette demo](../../images/command-palette-demo.gif)

## Configuration

The <xref:Whim.CommandPalette.CommandPaletteConfig> can be used to configure the command palette. The <xref:Whim.CommandPalette.CommandPaletteConfig.ActivationConfig> can be used to configure the default commands and matcher to use.

Custom instances of the command palette can be activated by calling <xref:Whim.CommandPalette.CommandPalettePlugin.Activate(Whim.CommandPalette.BaseVariantConfig)>.

## Commands

| Identifier                                                | Title                              | Keybind                                          |
| --------------------------------------------------------- | ---------------------------------- | ------------------------------------------------ |
| `whim.command_palette.toggle`                             | Toggle command palette             | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> |
| `whim.command_palette.activate_workspace`                 | Activate workspace                 | No default keybind                               |
| `whim.command_palette.rename_workspace`                   | Rename workspace                   | No default keybind                               |
| `whim.command_palette.create_workspace`                   | Create workspace                   | No default keybind                               |
| `whim.command_palette.move_window_to_workspace`           | Move window to workspace           | No default keybind                               |
| `whim.command_palette.move_multiple_windows_to_workspace` | Move multiple windows to workspace | No default keybind                               |
| `whim.command_palette.remove_window`                      | Select window to remove from Whim  | No default keybind                               |
