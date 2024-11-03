# Layout Engines

## Available layout engines

| Engine                                          | TL;DR                                                                    |
| ----------------------------------------------- | ------------------------------------------------------------------------ |
| [`FocusLayoutEngine`](#focuslayoutengine)       | One window at a time                                                     |
| [`SliceLayoutEngine`](#slicelayoutengine)       | `Awesome`/`dwm`-style dynamic tiling (primary/stack, multi-column, etc.) |
| [`TreeLayoutEngine`](#treelayoutengine)         | `i3`-style dynamic tiling (arbitrary grids)                              |
| [`FloatingLayoutEngine`](#floatinglayoutengine) | All windows are free-floating                                            |

### `FocusLayoutEngine`

<xref:Whim.FocusLayoutEngine> is a layout engine that displays one window at a time.

To focus the window in the specified direction, call <xref:Whim.ILayoutEngine.FocusWindowInDirection(Whim.Direction,Whim.IWindow)>.

To reorder windows, calling <xref:Whim.ILayoutEngine.SwapWindowInDirection(Whim.Direction,Whim.IWindow)> will swap the current window with the window in the specified direction. **This will not change the focused window.**

Windows which are not focused are minimized to the taskbar.

![FocusLayoutEngine demo](../../images/layout-engines/focus.gif)

### `SliceLayoutEngine`

<xref:Whim.SliceLayout.SliceLayoutEngine> is an `Awesome`/`dwm`-style layout engine, which arranges following a deterministic algorithm filling a grid configured in the config file.

Each `SliceLayoutEngine` is divided into a number of <xref:Whim.SliceLayout.IArea>s. There are three types of `IArea`s:

- <xref:Whim.SliceLayout.ParentArea>: An area that can have any `IArea` implementation as a child
- <xref:Whim.SliceLayout.SliceArea>: An ordered area that can have any `IWindow` as a child. There can be multiple `SliceArea`s in a `SliceLayoutEngine`, and they are ordered by the `Order` property/parameter.
- <xref:Whim.SliceLayout.OverflowArea>: An area that can have any infinite number of `IWindow`s as a child. There can be only one `OverflowArea` in a `SliceLayoutEngine` - any additional `OverflowArea`s will be ignored. If no `OverflowArea` is specified, the `SliceLayoutEngine` will replace the last `SliceArea` with an `OverflowArea`.

`OverflowArea`s are implicitly the last ordered area in the layout engine, in comparison to all `SliceArea`s.
Internally, `SliceLayoutEngine` stores a list of <xref:Whim.IWindow>s. Each `IArea` corresponds to a "slice" of the `IWindow` list.

#### Defining different `SliceLayouts`

The <xref:Whim.SliceLayout.SliceLayouts> static class contains methods to create a few common layouts:

- primary/stack (master/stack)
- column layout
- row layout
- multi-column layout, with arbitrary numbers of windows in each column
- three-column layout, with the middle column being the primary

```csharp
context.Store.Dispatch(
    new SetCreateLayoutEnginesTransform(
        () => new CreateLeafLayoutEngine[]
        {
            (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
            (id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
            (id) => CustomLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id)
        }
    )
);
```

Arbitrary layouts can be created by nesting areas:

```csharp
context.Store.Dispatch(
    new SetCreateLayoutEnginesTransform(
        () => new CreateLeafLayoutEngine[]
        {
            (id) => new SliceLayoutEngine(
                context,
                sliceLayoutPlugin,
                id,
                new ParentArea(
                    isRow: true,
                    (0.5, new OverflowArea()),
                    (0.5, new SliceArea(order: 0, maxChildren: 2))
                )
            ) { Name = "Overflow on left" }
        }
    )
);
```

`SliceLayoutEngine` requires the <xref:Whim.SliceLayout.SliceLayoutPlugin> to be added to the <xref:Whim.IPluginManager> instance:

```csharp
SliceLayoutPlugin sliceLayoutPlugin = new(context);
context.PluginManager.AddPlugin(sliceLayoutPlugin);
```

### `TreeLayoutEngine`

<xref:Whim.TreeLayout.TreeLayoutEngine> is a layout that allows users to create arbitrary grid layouts, similar to `i3`. Unlike [`SliceLayoutEngine`](#slicelayoutengine), windows can can be added in any location at runtime.

`TreeLayoutEngine` requires the <xref:Whim.TreeLayout.TreeLayoutPlugin> to be added to the <xref:Whim.IPluginManager> instance:

```csharp
TreeLayoutPlugin treeLayoutPlugin = new(context);
context.PluginManager.AddPlugin(treeLayoutPlugin);
```

![TreeLayoutEngine demo](../../images/layout-engines/tree.gif)

### `FloatingLayoutEngine`

<xref:Whim.FloatingWindow.FloatingLayoutEngine> is a layout that has all windows being free-floating. To have specific windows float within a different layout, see the [Floating Window Plugin](../plugins/floating-window.md).

![FloatingLayoutEngine demo](../../images/layout-engines/floating.gif)
