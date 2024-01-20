# Slice Layout Plugin

<xref:Whim.SliceLayout.SliceLayoutPlugin> provides commands and functionality for the <xref:Whim.SliceLayout.SliceLayoutEngine>.

<xref:Whim.SliceLayout.SliceLayoutPlugin> does not load the <xref:Whim.SliceLayout.SliceLayoutEngine> - that is done when creating a workspace via <xref:Whim.SliceLayout.IWorkspaceManager.Add>.

For more about the `SliceLayoutEngine`, see the [Layout Engines](../layout-engines.md#xrefwhimslicelayoutslicelayoutengine) page.

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

## Commands

| Identifier                                    | Title                              | Keybind            |
| --------------------------------------------- | ---------------------------------- | ------------------ |
| `whim.slice_layout.set_insertion_type.swap`   | Set slice insertion type to swap   | No default keybind |
| `whim.slice_layout.set_insertion_type.rotate` | Set slice insertion type to rotate | No default keybind |
| `whim.slice_layout.window.promote`            | Promote window in stack            | No default keybind |
| `whim.slice_layout.window.demote`             | Demote window in stack             | No default keybind |
| `whim.slice_layout.focus.promote`             | Promote focus in stack             | No default keybind |
| `whim.slice_layout.focus.demote`              | Demote focus in stack              | No default keybind |
