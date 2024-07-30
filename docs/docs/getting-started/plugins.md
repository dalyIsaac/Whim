# Plugins

Whim is build around plugins. Plugins are referenced using `#r` and `using` statements at the top of the config file. Each plugin generally has a `Config` class, and a `Plugin` class, and needs to be added to the <xref:Whim.IPluginManager>. For example:

```csharp
BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
BarPlugin barPlugin = new(context, barConfig);
context.PluginManager.AddPlugin(barPlugin);
```

| Plugin                                                                   | TL;DR                                                                                             |
|--------------------------------------------------------------------------| ------------------------------------------------------------------------------------------------- |
| [Bar](../plugins/bar.md)                                                 | Adds a configurable bar to the top of each screen                                                 |
| [Command Palette](../plugins/command-palette.md)                         | Fuzzy command palette filled with [commands](../customize/commands.md)                            |
| [Proxy Floating Layout](../plugins/proxy-floating-layout.md)             | Lets windows float outside other layout engines                                                   |
| [Focus](../plugins/focus-indicator.md)                                   | Adds a border around the current window                                                           |
| [Gaps](../plugins/gaps.md)                                               | Adds gaps between windows                                                                         |
| [Layout Preview](../plugins/layout-preview.md)                           | Shows a preview when dragging windows                                                             |
| [Slice Layout](../plugins/slice-layout.md)                               | Plugin for the [`SliceLayoutEngine`](../customize/layout-engines.md#slicelayoutengine)            |
| [Tree Layout](../plugins/tree-layout.md)                                 | Plugin for the [`TreeLayoutEngine`](../customize//layout-engines.md#treelayoutengine)             |
| [Tree Layout Bar](../plugins/tree-layout-bar.md)                         | Provides a widget for the [Bar](../plugins/bar.md), for the `TreeLayoutEngine`                    |
| [Tree Layout Command Palette](../plugins/tree-layout-command-palette.md) | Adds `TreeLayoutEngine`-specific commands to the [Command Palette](../plugins/command-palette.md) |
| [Updater](../plugins/updater.md)                                         | Plugin to automatically update Whim                                                               |
