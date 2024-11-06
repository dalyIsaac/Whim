# Workspaces

[!INCLUDE[Workspace overview](../../_includes/core/workspace-overview.md)]

## Configuration

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

## Sticky Workspaces

Sticky workspaces are being worked on in [this GitHub issue](https://github.com/dalyIsaac/Whim/issues/660).

👷🏗️🚧
