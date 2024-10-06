# Slice Layout Plugin

<xref:Whim.SliceLayout.SliceLayoutPlugin> provides commands and functionality for the <xref:Whim.SliceLayout.SliceLayoutEngine>.

<xref:Whim.SliceLayout.SliceLayoutPlugin> does not load the <xref:Whim.SliceLayout.SliceLayoutEngine> - that is done when creating a workspace via the <xref:Whim.AddWorkspaceTransform>.

For more about the `SliceLayoutEngine`, see the [Layout Engines](../core/layout-engines.md#slicelayoutengine) page.

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.SliceLayout\Whim.SliceLayout.dll"

using Whim;
using Whim.SliceLayout;

void DoConfig(IContext context)
{
  // ...

  SliceLayoutPlugin sliceLayoutPlugin = new(context);
  context.PluginManager.AddPlugin(sliceLayoutPlugin);

  // ...
}

return DoConfig;
```

[!INCLUDE [Commands](../../_includes/plugins/slice-layout.md)]
