# Tree Layout Bar Plugin

<xref:Whim.TreeLayout.Bar.TreeLayoutBarPlugin> contains the tree layout engine widget for the [bar plugin](./bar.md).

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"

using Whim;
using Whim.Bar
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;

void DoConfig(IContext context)
{
  // ...

  // Tree layout plugin
  TreeLayoutPlugin treeLayoutPlugin = new(context);
  context.PluginManager.AddPlugin(treeLayoutPlugin);

  // Tree layout bar
  TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
  context.PluginManager.AddPlugin(treeLayoutBarPlugin);
  rightComponents.Add(treeLayoutBarPlugin.CreateComponent());

  // Bar plugin
  List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
  List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
  List<BarComponent> rightComponents = new()
  {
    BatteryWidget.CreateComponent(),
    ActiveLayoutWidget.CreateComponent(),
    DateTimeWidget.CreateComponent(),
  };

  BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
  BarPlugin barPlugin = new(context, barConfig);
  context.PluginManager.AddPlugin(barPlugin);

  // ...
}

return DoConfig;
```

## Commands

N/A
