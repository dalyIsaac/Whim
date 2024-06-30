# Layout Engines

A "layout engine" or <xref:Whim.ILayoutEngine> in Whim is responsible for arranging windows in a workspace. Each workspace has a single active layout engine, and can cycle through different layout engines.

There are two different types of layout engines:

- proxy layout engines
- leaf layout engines

**Proxy layout engines** wrap other engines, and can be used to modify the behavior of other engines. For example, the [`Gaps` plugin](../plugins/gaps.md) will add gaps between windows - normally layout engines won't leave gaps between windows.

**Leaf layout engines** are the lowest level layout engines, and are responsible for actually arranging windows.

## Available layout engines

| Engine                                    | TL;DR                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------ |
| [`FocusLayoutEngine`](#focuslayoutengine) | One window at a time                                                     |
| [`SliceLayoutEngine`](#slicelayoutengine) | `Awesome`/`dwm`-style dynamic tiling (primary/stack, multi-column, etc.) |
| [`TreeLayoutEngine`](#treelayoutengine)   | `i3`-style dynamic tiling (arbitrary grids)                              |

### `FocusLayoutEngine`

<xref:Whim.FocusLayoutEngine> is a layout engine that displays one window at a time:

To focus the window in the specified direction, call <xref:Whim.ILayoutEngine.FocusWindowInDirection(Whim.Direction,Whim.IWindow)>.

To reorder windows, calling <xref:Whim.ILayoutEngine.SwapWindowInDirection(Whim.Direction,Whim.IWindow)> will swap the current window with the window in the specified direction. **This will not change the focused window.**

Windows which are not focused are minimized to the taskbar.

![FocusLayoutEngine demo](../../images/focus-layout-demo.gif)

### `SliceLayoutEngine`

<xref:Whim.SliceLayout.SliceLayoutEngine> is an `Awesome`/`dwm`-style layout engine, which arranges following a deterministic algorithm filling a grid configured in the config file.

Each `SliceLayoutEngine` is divided into a number of <xref:Whim.SliceLayout.IArea>s. There are three types of `IArea`s:

- <xref:Whim.SliceLayout.ParentArea>: An area that can have any `IArea` implementation as a child
- <xref:Whim.SliceLayout.SliceArea>: An ordered area that can have any `IWindow` as a child. There can be multiple `SliceArea`s in a `SliceLayoutEngine`, and they are ordered by the `Order` property/parameter.
- <xref:Whim.SliceLayout.OverflowArea>: An area that can have any infinite number of `IWindow`s as a child. There can be only one `OverflowArea` in a `SliceLayoutEngine` - any additional `OverflowArea`s will be ignored. If no `OverflowArea` is specified, the `SliceLayoutEngine` will replace the last `SliceArea` with an `OverflowArea`.

`OverflowArea`s are implicitly the last ordered area in the layout engine, in comparison to all `SliceArea`s.
Internally, `SliceLayoutEngine` stores a list of <xref:Whim.IWindow>s. Each `IArea` corresponds to a "slice" of the `IWindow` list.

#### Defining `SliceLayoutEngine`s

The `SliceLayouts` contains methods to create a few common layouts:

- primary/stack (master/stack)
- multi-column layout
- three-column layout, with the middle column being the primary

```csharp
context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
{
    (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
    (id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
    (id) => CustomLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id)
}
```

Arbitrary layouts can be created by nesting areas:

```csharp
context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
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
};
```

`SliceLayoutEngine` requires the <xref:Whim.SliceLayout.SliceLayoutPlugin> to be added to the <xref:Whim.IPluginManager> instance:

```csharp
SliceLayoutPlugin sliceLayoutPlugin = new(context);
context.PluginManager.AddPlugin(sliceLayoutPlugin);
```

![SliceLayoutEngine demo](../../images/slice-layout-demo.gif)

### `TreeLayoutEngine`

<xref:Whim.TreeLayout.TreeLayoutEngine> is a layout that allows users to create arbitrary grid layouts, similar to `i3`. Unlike [`SliceLayoutEngine`](#slicelayoutengine), windows can can be added in any location at runtime.

`TreeLayoutEngine` requires the <xref:Whim.TreeLayout.TreeLayoutPlugin> to be added to the <xref:Whim.IPluginManager> instance:

```csharp
TreeLayoutPlugin treeLayoutPlugin = new(context);
context.PluginManager.AddPlugin(treeLayoutPlugin);
```

![TreeLayoutEngine demo](../../images/tree-layout-demo.gif)

### `FreeLayoutEngine`

<xref:Whim.Layout.FreeEngineLayout> is a layout that allows users to have only free-floating windows.

![FreeLayoutEngine demo](../../images/free-layout-demo.gif)
