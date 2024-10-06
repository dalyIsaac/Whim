# Gaps Plugin

The <xref:Whim.Gaps.GapsPlugin> adds the config and commands for the <xref:Whim.Gaps.GapsLayoutEngine> proxy layout engine to add gaps between each of the windows in the layout.

![Gaps plugin demo](../../images/gaps-demo.png)

This behavior can be customized with the <xref:Whim.Gaps.GapsConfig> provided to the <xref:Whim.Gaps.GapsPlugin> constructor.

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"

using Whim;
using Whim.Gaps;

void DoConfig(IContext context)
{
  // ...

  GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 10 };
  GapsPlugin gapsPlugin = new(context, gapsConfig);
  context.PluginManager.AddPlugin(gapsPlugin);

  // ...
}

return DoConfig;
```

[!INCLUDE [Commands](../../_includes/plugins/gaps.md)]
