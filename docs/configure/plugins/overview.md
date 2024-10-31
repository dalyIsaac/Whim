# Plugins Overview

Whim is built around plugins. They provide additional functionality to Whim, such as a bar, floating windows, or a command palette. Plugins are loaded as required by Whim based on the configuration.

| Plugin                      | TL;DR                                                                                  | Docs                                                                 |
| --------------------------- | -------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| Bar                         | Adds a configurable bar to the top of each screen                                      | [`plugins.bar`](bar.md)                                              |
| Command Palette             | Fuzzy command palette filled with [commands](../core/commands.md)                      | [`plugins.command_palette`](command-palette.md)                      |
| Floating Window             | Lets windows float outside other layout engines                                        | [`plugins.floating_window`](floating-window.md)                      |
| Focus                       | Adds a border around the current window                                                | [`plugins.focus_indicator`](focus-indicator.md)                      |
| Gaps                        | Adds gaps between windows                                                              | [`plugins.gaps`](gaps.md)                                            |
| Layout Preview              | Shows a preview when dragging windows                                                  | [`plugins.layout_preview`](layout-preview.md)                        |
| Slice Layout                | Plugin for the [`SliceLayoutEngine`](../core/layout-engines.md#slice)                  | [`plugins.slice_layout`](slice-layout.md)                            |
| Tree Layout                 | Plugin for the [`TreeLayoutEngine`](../core/layout-engines.md#tree)                    | [`plugins.tree_layout`](tree-layout.md)                              |
| Tree Layout Bar             | Provides a widget for the [Bar](bar.md), for the `TreeLayoutEngine`                    | [`plugins.bar`](bar.md#tree-layout-widget)                           |
| Tree Layout Command Palette | Adds `TreeLayoutEngine`-specific commands to the [Command Palette](command-palette.md) | [`plugins.command_palette`](command-palette.md#tree-layout-commands) |
| Updater                     | Plugin to automatically check for updates                                              | [`plugins.updater`](updater.md)                                      |
