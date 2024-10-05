# Plugins

Whim is build around plugins. Plugins are referenced using `#r` and `using` statements at the top of the config file. Each plugin generally has a `Config` class, and a `Plugin` class, and needs to be added to the <xref:Whim.IPluginManager>. For example:

```csharp
BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
BarPlugin barPlugin = new(context, barConfig);
context.PluginManager.AddPlugin(barPlugin);
```

| Plugin                      | TL;DR                                                                                             | YAML JSON Docs                                                                               | C# Scripting Docs                                                                   |
| --------------------------- | ------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| Bar                         | Adds a configurable bar to the top of each screen                                                 | [`plugins.bar`](../customize/plugins/bar.md)                                                 | [`Whim.Bar`](../script/plugins/bar.md)                                              |
| Command Palette             | Fuzzy command palette filled with [commands](../customize/commands.md)                            | [`plugins.command_palette`](../customize/plugins/command-palette.md)                         | [`Whim.CommandPalette`](../script/plugins/command-palette.md)                       |
| Floating Window             | Lets windows float outside other layout engines                                                   | [`plugins.floating_window`](../customize/plugins/floating-window.md)                         | [`Whim.FloatingWindow`](../script/plugins/floating-window.md)                       |
| Focus                       | Adds a border around the current window                                                           | [`plugins.focus_indicator`](../customize/plugins/focus-indicator.md)                         | [`Whim.FocusIndicator`](../script/plugins/focus-indicator.md)                       |
| Gaps                        | Adds gaps between windows                                                                         | [`plugins.gaps`](../customize/plugins/gaps.md)                                               | [`Whim.Gaps`](../script/plugins/gaps.md)                                            |
| Layout Preview              | Shows a preview when dragging windows                                                             | [`plugins.layout_preview`](../customize/plugins/layout-preview.md)                           | [`Whim.LayoutPreview`](../script/plugins/layout-preview.md)                         |
| Slice Layout                | Plugin for the [`SliceLayoutEngine`](../customize/layout-engines.md#slicelayoutengine)            | [`plugins.slice_layout`](../customize/plugins/slice-layout.md)                               | [`Whim.SliceLayout`](../script/plugins/slice-layout.md)                             |
| Tree Layout                 | Plugin for the [`TreeLayoutEngine`](../customize//layout-engines.md#treelayoutengine)             | [`plugins.tree_layout`](../customize/plugins/tree-layout.md)                                 | [`Whim.TreeLayout`](../script/plugins/tree-layout.md)                               |
| Tree Layout Bar             | Provides a widget for the [Bar](../plugins/bar.md), for the `TreeLayoutEngine`                    | [`plugins.tree_layout_bar`](../customize/plugins/tree-layout-bar.md)                         | [`Whim.TreeLayoutBar`](../script/plugins/tree-layout-bar.md)                        |
| Tree Layout Command Palette | Adds `TreeLayoutEngine`-specific commands to the [Command Palette](../plugins/command-palette.md) | [`plugins.tree_layout_command_palette`](../customize/plugins/tree-layout-command-palette.md) | [`Whim.TreeLayoutCommandPalette`](../script/plugins/tree-layout-command-palette.md) |
| Updater                     | Plugin to automatically update Whim                                                               | [`plugins.updater`](../customize/plugins/updater.md)                                         | [`Whim.Updater`](../script/plugins/updater.md)                                      |
