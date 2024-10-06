# Workspaces

A "workspace" or <xref:Whim.IWorkspace> in Whim is a collection of windows displayed on a single monitor. The layouts of workspaces are determined by their [layout engines](layout-engines.md). Each workspace has a single active layout engine, and can cycle through different layout engines.

The <xref:Whim.SetCreateLayoutEnginesTransform> lets you specify the default layout engines for workspaces. For example, the following config sets up three workspaces, and two layout engines:

```csharp
// Set up workspaces.
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Browser")).Value;
Guid ideWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("IDE")).Value;
Guid altWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Alt")).Value;

// Set up layout engines.
context.Store.Dispatch(
    new SetCreateLayoutEnginesTransform(
        () => new CreateLeafLayoutEngine[]
        {
            (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
            (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id)
        }
    )
);
```

It's also possible to customize the layout engines for a specific workspace:

```csharp
context.Store.Dispatch(
    new AddWorkspaceTransform(
        "Alt",
        new CreateLeafLayoutEngine[]
        {
            (id) => new FocusLayoutEngine(id)
        }
    )
);
```

The available layout engines are listed in [Layout Engines](./layout-engines.md).

If no name is provided, the name will default to `Workspace {workspaces.Count + 1}.`

When Whim exits, it will save the current workspaces and the current positions of each window within them. When Whim is started again, it will attempt to merge the saved workspaces with the workspaces defined in the config.

## Example Command

```csharp
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Browser")).Value;
Guid ideWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("IDE")).Value;

context.CommandManager.Add(
    "merge_workspace_with_browser",
    "Merge current workspace with Browser workspace",
    () => {
        Guid activeWorkspaceId = context.Store.Pick(Pickers.PickActiveWorkspaceId());
        context.Store.Dispatch(new MergeWorkspaceTransform(activeWorkspaceId, browserWorkspaceId));
    }
);
```

For more, see the [Store](./store.md) and [Commands](../../configure/core/commands.md) pages.
