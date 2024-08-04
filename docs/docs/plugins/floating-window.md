# Floating Window Plugin

The <xref:Whim.FloatingLayout.FloatingWindowPlugin> adds a `ProxyFloatingLayoutEngine` proxy engine to Whim. This adds the ability for windows to float outside of any other layouts. If you want to have a layout of free-floating windows, see the <xref:Whim.FloatingLayout.FloatingLayoutEngine>.

The <xref:Whim.FloatingLayout.FloatingWindowPlugin> has no configuration options.

![Floating window demo](../../images/floating-window-demo.gif)

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"

using Whim;
using Whim.FloatingLayout;

void DoConfig(IContext context)
{
  // ...

  FloatingWindowPlugin floatingWindowPlugin = new(context);
  context.PluginManager.AddPlugin(floatingWindowPlugin);

  // ...
}

return DoConfig;
```

## Commands

| Identifier                                     | Title                   | Keybind                                          |
|------------------------------------------------| ----------------------- | ------------------------------------------------ |
| `whim.floating_window.toggle_window_floating`  | Toggle window floating  | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>F</kbd> |
| `whim.floating_window.mark_window_as_floating` | Mark window as floating | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>M</kbd> |
| `whim.floating_window.mark_window_as_docked`   | Mark window as docked   | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>D</kbd> |
