# Tree Layout Plugin

<xref:Whim.TreeLayout.TreeLayoutPlugin> provides commands and functionality for the <xref:Whim.TreeLayout.TreeLayoutEngine>.

<xref:Whim.TreeLayout.TreeLayoutPlugin> does not load the <xref:Whim.TreeLayout.TreeLayoutEngine> - that is done when creating a workspace via <xref:Whim.TreeLayout.IWorkspaceManager.Add>.

For more about the `TreeLayoutEngine`, see the [Layout Engines](../layout-engines.md#xrefwhimtreelayouttreelayoutengine) page.

## Commands

| Identifier                                  | Title                                          | Keybind                                                                |
| ------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------- |
| `whim.tree_layout.add_tree_direction_left`  | Add windows to the left of the current window  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.tree_layout.add_tree_direction_right` | Add windows to the right of the current window | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.tree_layout.add_tree_direction_up`    | Add windows above the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>UP</kbd>    |
| `whim.tree_layout.add_tree_direction_down`  | Add windows below the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>DOWN</kbd>  |
