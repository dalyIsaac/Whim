# Introduction

> All the power to the user!

Whim is an able, hackable, pluggable and scriptable window manager for Windows 10 and 11, built using WinUI 3, .NET, and C# scripting.

![Whim demo](images/demo.gif)

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
