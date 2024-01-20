# Tree Layout Plugin

<xref:Whim.TreeLayout.TreeLayoutPlugin> provides commands and functionality for the <xref:Whim.TreeLayout.TreeLayoutEngine>.

<xref:Whim.TreeLayout.TreeLayoutPlugin> does not load the <xref:Whim.TreeLayout.TreeLayoutEngine> - that is done when creating a workspace via <xref:Whim.IWorkspaceManager.Add(System.String,System.Collections.Generic.IEnumerable{Whim.CreateLeafLayoutEngine})>.

For more about the `TreeLayoutEngine`, see the [Layout Engines](../layout-engines.md#treelayoutengine) page.

The [Tree Layout Bar plugin](./tree-layout-bar.md) provides a widget for the bar to set the direction to add the next window.

## Example Config`

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

## Commands

| Identifier                                  | Title                                          | Keybind                                                                |
| ------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------- |
| `whim.tree_layout.add_tree_direction_left`  | Add windows to the left of the current window  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.tree_layout.add_tree_direction_right` | Add windows to the right of the current window | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.tree_layout.add_tree_direction_up`    | Add windows above the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>UP</kbd>    |
| `whim.tree_layout.add_tree_direction_down`  | Add windows below the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>DOWN</kbd>  |
