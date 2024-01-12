# Layout Engines

## <xref:Whim.FocusLayoutEngine>

`FocusLayoutEngine` is a layout engine that displays one window at a time:

- Calling <Whim.ILayoutEngine.SwapWindowInDirection> will swap the current window with the window in the specified direction.
- Calling <Whim.ILayoutEngine.FocusWindowInDirection> will focus the window in the specified direction.

Windows which are not focused are minimized to the taskbar.

<!-- TODO: gif -->

## <xref:Whim.SliceLayout.SliceLayoutEngine>

`SliceLayoutEngine` is a layout engine that internally stores an ordered list of <xref:Whim.IWindow>s. The monitor is divided into a number of <xref:Whim.SliceLayout.IArea>s. Each `IArea`corresponds to a "slice" of the`IWindow` list.

There are three types of `IArea`s:

- <xref:Whim.SliceLayout.ParentArea>: An area that can have any `IArea` implementation as a child
- <xref:Whim.SliceLayout.SliceArea>: An ordered area that can have any `IWindow` as a child. There can be multiple `SliceArea`s in a `SliceLayoutEngine`, and they are ordered by the `Order` property/parameter.
- <xref:Whim.SliceLayout.OverflowArea>: An area that can have any infinite number of `IWindow`s as a child. There can be only one `OverflowArea` in a `SliceLayoutEngine` - any additional `OverflowArea`s will be ignored. If no `OverflowArea` is specified, the `SliceLayoutEngine` will replace the last `SliceArea` with an `OverflowArea`.

`OverflowArea`s are implicitly the last ordered area in the layout engine, in comparison to all `SliceArea`s.

The `SliceLayouts` contains methods to create a few common layouts:

- primary/stack (master/stack)
- multi-column layout
- three-column layout, with the middle column being the primary

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
    ) { Name = "Overflow on left" },

    (id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
    (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id)
};
```

`SliceLayoutEngine` requires the <xref:Whim.SliceLayout.SliceLayoutPlugin> to be added to the <xref:Whim.IPluginManager> instance:

```csharp
SliceLayoutPlugin sliceLayoutPlugin = new(context);
context.PluginManager.AddPlugin(sliceLayoutPlugin);
```

<!-- TODO: gif -->

## <xref:Whim.TreeLayout.TreeLayoutEngine>

`TreeLayoutEngine` is a layout that allows users to create arbitrary grid layouts. Unlike `SliceLayoutEngine`, windows can can be added in any location.

`TreeLayoutEngine` requires the <xref:Whim.TreeLayout.TreeLayoutPlugin> to be added to the <xref:Whim.IPluginManager> instance:

```csharp
TreeLayoutPlugin treeLayoutPlugin = new(context);
context.PluginManager.AddPlugin(treeLayoutPlugin);
```

<!-- TODO: gif -->
