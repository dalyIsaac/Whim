# Tree Layout Plugin

<xref:Whim.TreeLayout.TreeLayoutPlugin> provides commands and functionality for the <xref:Whim.TreeLayout.TreeLayoutEngine>.

<xref:Whim.TreeLayout.TreeLayoutPlugin> does not load the <xref:Whim.TreeLayout.TreeLayoutEngine> - that is done when creating a workspace via the <xref:Whim.AddWorkspaceTransform>.

For more about the `TreeLayoutEngine`, see the [Layout Engines](../../configure/core/layout-engines.md#tree) page.

The [Tree Layout Bar plugin](./tree-layout-bar.md) provides a widget for the bar to set the direction to add the next window.

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"

using Whim;
using Whim.TreeLayout;

void DoConfig(IContext context)
{
  // ...

  TreeLayoutPlugin treeLayoutPlugin = new(context);
  context.PluginManager.AddPlugin(treeLayoutPlugin);

  // ...
}

return DoConfig;
```

[!INCLUDE [Commands](../../_includes/plugins/tree-layout.md)]
