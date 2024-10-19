# Floating Window Plugin

The <xref:Whim.FloatingWindow.FloatingWindowPlugin> adds a `ProxyFloatingLayoutEngine` proxy engine to Whim. This adds the ability for windows to float outside of any other layouts. If you want to have a layout of free-floating windows, see the <xref:Whim.FloatingWindow.FloatingLayoutEngine>.

The <xref:Whim.FloatingWindow.FloatingWindowPlugin> has no configuration options.

![Floating window demo](../../images/floating-window-demo.gif)

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.FloatingWindow\Whim.FloatingWindow.dll"

using Whim;
using Whim.FloatingWindow;

void DoConfig(IContext context)
{
  // ...

  FloatingWindowPlugin floatingWindowPlugin = new(context);
  context.PluginManager.AddPlugin(floatingWindowPlugin);

  // ...
}

return DoConfig;
```

[!INCLUDE [Commands](../../_includes/plugins/floating-window.md)]
