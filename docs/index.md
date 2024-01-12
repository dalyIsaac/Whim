# Introduction

> All the power to the user!

Whim is an hackable, pluggable and scriptable dynamic window manager for Windows 10 and 11, built using WinUI 3, .NET, and C# scripting.

![Whim demo](images/demo.gif)

## Why use Whim?

A window manager is responsible for controlling the layout of windows in your desktop environment. Whim is a [dynamic window manager](https://en.wikipedia.org/wiki/Dynamic_window_manager), where windows are arranged according to different layouts.

Whim supports multiple layout engines. Each [workspace](concepts/workspaces.md) can switch between different layout engines. For example, the `TreeLayoutEngine` allows users to create arbitrary grids of windows, while the `FocusLayoutEngine` allows users to focus on a single window at a time. For more, see [Layout Engines](layout-engines.md).

Whim is configured using C# scripting - no YAML to be found here. This means you can use the full power of C# to configure Whim. Whim also exposes its API for plugins to use. Accordingly, much of the more custom functionality has been implemented as plugins which users can choose to use or not.

Whim works by sitting on top of Windows' own window manager. It listens to window events and moves windows accordingly.

To see how Whim compares to other Windows window managers, see [Whim vs. Other Window Managers](comparison.md).

## Installation

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases) at GitHub.

## Getting Started

When you run Whim for the first time, it will create a `.whim` directory in your user profile - for example, `C:\Users\Isaac\.whim`. This can be configured with the CLI option `--dir`.

This directory will contain a `whim.config.csx` file which you can edit to customize Whim. This file is a C# script file, and is reloaded every time Whim starts. To have the best development experience, you should have dotnet tooling installed (Visual Studio Code will prompt you when you open `.whim`).

The config contains a pre-filled example which you can use as a starting point. You can also find the config on [GitHub](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx).

```csharp
...

void DoConfig(IContext context)
{
  context.Logger.Config = new LoggerConfig();

  // Bar plugin.
  List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
  List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
  List<BarComponent> rightComponents = new()
  {
    BatteryWidget.CreateComponent(),
    ActiveLayoutWidget.CreateComponent(),
    DateTimeWidget.CreateComponent()
  };

  BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
  BarPlugin barPlugin = new(context, barConfig);
  context.PluginManager.AddPlugin(barPlugin);

  ...

  // Set up workspaces.
  context.WorkspaceManager.Add("1");
  context.WorkspaceManager.Add("2");
  context.WorkspaceManager.Add("3");
  context.WorkspaceManager.Add("4");

  // Set up layout engines.
  context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
  {
    (id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
    (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
    (id) => SliceLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id),
    (id) => new FocusLayoutEngine(id),
    (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id),
    (id) => new ColumnLayoutEngine(id)
  };
}
```
