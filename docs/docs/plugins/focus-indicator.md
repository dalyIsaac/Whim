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

## Commands

| Identifier                            | Title                                         | Keybind            |
| ------------------------------------- | --------------------------------------------- | ------------------ |
| `whim.focus_indicator.show`           | Show focus indicator                          | No default keybind |
| `whim.focus_indicator.toggle`         | Toggle focus indicator                        | No default keybind |
| `whim.focus_indicator.toggle_fade`    | Toggle whether the focus indicator fades      | No default keybind |
| `whim.focus_indicator.toggle_enabled` | Toggle whether the focus indicator is enabled | No default keybind |
