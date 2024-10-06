# Plugins Overview

Whim is build around plugins. Plugins are loaded as required by Whim based on the configuration.

| Plugin                      | TL;DR                                                                                  | Docs                                                        |
| --------------------------- | -------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| Bar                         | Adds a configurable bar to the top of each screen                                      | [`plugins.bar`](bar.md)                                     |
| Command Palette             | Fuzzy command palette filled with [commands](../core/commands.md)                      | [`plugins.command_palette`](command-palette.md)             |
| Floating Window             | Lets windows float outside other layout engines                                        | [`plugins.floating_window`](floating-window.md)             |
| Focus                       | Adds a border around the current window                                                | [`plugins.focus_indicator`](focus-indicator.md)             |
| Gaps                        | Adds gaps between windows                                                              | [`plugins.gaps`](gaps.md)                                   |
| Layout Preview              | Shows a preview when dragging windows                                                  | [`plugins.layout_preview`](layout-preview.md)               |
| Slice Layout                | Plugin for the [`SliceLayoutEngine`](../core/layout-engines.md#slicelayoutengine)      | [`plugins.slice_layout`](slice-layout.md)                   |
| Tree Layout                 | Plugin for the [`TreeLayoutEngine`](../core/layout-engines.md#treelayoutengine)        | [`plugins.tree_layout`](tree-layout.md)                     |
| Tree Layout Bar             | Provides a widget for the [Bar](bar.md), for the `TreeLayoutEngine`                    | [`plugins.bar`](bar.md#tree-layout)                         |
| Tree Layout Command Palette | Adds `TreeLayoutEngine`-specific commands to the [Command Palette](command-palette.md) | [`plugins.command_palette`](command-palette.md#tree-layout) |
| Updater                     | Plugin to automatically update Whim                                                    | [`plugins.updater`](updater.md)                             |
