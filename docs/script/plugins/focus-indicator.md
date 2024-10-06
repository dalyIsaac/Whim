# Focus Indicator Plugin

The <xref:Whim.FocusIndicator.FocusIndicatorPlugin> adds a border around the current window which Whim has tracked as having focus. Various options can be configured using the <xref:Whim.FocusIndicator.FocusIndicatorConfig>.

> [!WARNING]
> The focus may remain on a window despite the focus shifting to an untracked window (e.g., the Start Menu). This depends on the config option <xref:Whim.IRouterManager.RouterOptions>.

![Focus indicator demo](../../images/focus-indicator-demo.gif)

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"

using Whim;
using Whim.FocusIndicator;

void DoConfig(IContext context)
{
  // ...

  FocusIndicatorConfig focusIndicatorConfig = new() { Color = new SolidColorBrush(Colors.Red), FadeEnabled = true };
  FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
  context.PluginManager.AddPlugin(focusIndicatorPlugin);

  // ...
}

return DoConfig;
```

[!INCLUDE [Commands](../../_common/plugins/focus-indicator.md)]
