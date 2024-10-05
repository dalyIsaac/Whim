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

| Plugin                      | TL;DR                                                                                             | C# Scripting Docs                                                                   |
| --------------------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| Bar                         | Adds a configurable bar to the top of each screen                                                 | [`Whim.Bar`](../script/plugins/bar.md)                                              |
| Command Palette             | Fuzzy command palette filled with [commands](../configurecommands.md)                             | [`Whim.CommandPalette`](../script/plugins/command-palette.md)                       |
| Floating Window             | Lets windows float outside other layout engines                                                   | [`Whim.FloatingWindow`](../script/plugins/floating-window.md)                       |
| Focus                       | Adds a border around the current window                                                           | [`Whim.FocusIndicator`](../script/plugins/focus-indicator.md)                       |
| Gaps                        | Adds gaps between windows                                                                         | [`Whim.Gaps`](../script/plugins/gaps.md)                                            |
| Layout Preview              | Shows a preview when dragging windows                                                             | [`Whim.LayoutPreview`](../script/plugins/layout-preview.md)                         |
| Slice Layout                | Plugin for the [`SliceLayoutEngine`](../configurelayout-engines.md#slicelayoutengine)             | [`Whim.SliceLayout`](../script/plugins/slice-layout.md)                             |
| Tree Layout                 | Plugin for the [`TreeLayoutEngine`](../configure/layout-engines.md#treelayoutengine)              | [`Whim.TreeLayout`](../script/plugins/tree-layout.md)                               |
| Tree Layout Bar             | Provides a widget for the [Bar](../plugins/bar.md), for the `TreeLayoutEngine`                    | [`Whim.TreeLayoutBar`](../script/plugins/tree-layout-bar.md)                        |
| Tree Layout Command Palette | Adds `TreeLayoutEngine`-specific commands to the [Command Palette](../plugins/command-palette.md) | [`Whim.TreeLayoutCommandPalette`](../script/plugins/tree-layout-command-palette.md) |
| Updater                     | Plugin to automatically update Whim                                                               | [`Whim.Updater`](../script/plugins/updater.md)                                      |
