# Floating Layout Plugin

The <xref:Whim.FloatingLayout.FloatingLayoutPlugin> adds a `FloatingLayoutEngine` proxy engine to Whim. This adds the ability for windows to float outside of any other layouts.

The <xref:Whim.FloatingLayout.FloatingLayoutPlugin> has no configuration options.

![Floating layout demo](../../images/floating-layout-demo.gif)

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"

using Whim;
using Whim.FloatingLayout;

void DoConfig(IContext context)
{
  // ...

  FloatingLayoutPlugin floatingLayoutPlugin = new(context);
  context.PluginManager.AddPlugin(floatingLayoutPlugin);

  // ...
}

return DoConfig;
```

## Commands

| Identifier                                     | Title                   | Keybind                                          |
| ---------------------------------------------- | ----------------------- | ------------------------------------------------ |
| `whim.floating_layout.toggle_window_floating`  | Toggle window floating  | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>F</kbd> |
| `whim.floating_layout.mark_window_as_floating` | Mark window as floating | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>M</kbd> |
| `whim.floating_layout.mark_window_as_docked`   | Mark window as docked   | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>D</kbd> |
