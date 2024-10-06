# Plugins Overview

Whim is build around plugins. Plugins are referenced using `#r` and `using` statements at the top of the config file. Each plugin generally has a `Config` class, and a `Plugin` class, and needs to be added to the <xref:Whim.IPluginManager>. For example:

```csharp
BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
BarPlugin barPlugin = new(context, barConfig);
context.PluginManager.AddPlugin(barPlugin);
```

If a plugin was loaded by the YAML/JSON configuration, it will be loaded automatically and does not need to be added to the `PluginManager`. Such plugins can be access via:

```csharp
BarPlugin barPlugin = (BarPlugin)context.PluginManager.LoadedPlugins.First(p => p.Name == "whim.bar");
```

## Plugins

| Plugin                      | TL;DR                                                                                                                     | C# Scripting Docs                                                 |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Bar                         | Adds a configurable bar to the top of each screen                                                                         | [`Whim.Bar`](bar.md)                                              |
| Command Palette             | Fuzzy command palette filled with [commands](../../configure/core/commands.md) and [custom commands](../core/commands.md) | [`Whim.CommandPalette`](command-palette.md)                       |
| Floating Window             | Lets windows float outside other layout engines                                                                           | [`Whim.FloatingWindow`](floating-window.md)                       |
| Focus                       | Adds a border around the current window                                                                                   | [`Whim.FocusIndicator`](focus-indicator.md)                       |
| Gaps                        | Adds gaps between windows                                                                                                 | [`Whim.Gaps`](gaps.md)                                            |
| Layout Preview              | Shows a preview when dragging windows                                                                                     | [`Whim.LayoutPreview`](layout-preview.md)                         |
| Slice Layout                | Plugin for the [`SliceLayoutEngine`](../core/layout-engines.md#slicelayoutengine)                                         | [`Whim.SliceLayout`](slice-layout.md)                             |
| Tree Layout                 | Plugin for the [`TreeLayoutEngine`](../core/layout-engines.md#treelayoutengine)                                           | [`Whim.TreeLayout`](tree-layout.md)                               |
| Tree Layout Bar             | Provides a widget for the [Bar](bar.md), for the `TreeLayoutEngine`                                                       | [`Whim.TreeLayoutBar`](tree-layout-bar.md)                        |
| Tree Layout Command Palette | Adds `TreeLayoutEngine`-specific commands to the [Command Palette](command-palette.md)                                    | [`Whim.TreeLayoutCommandPalette`](tree-layout-command-palette.md) |
| Updater                     | Plugin to automatically update Whim                                                                                       | [`Whim.Updater`](updater.md)                                      |
