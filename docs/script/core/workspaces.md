# Workspaces

[!INCLUDE[Workspace overview](../../_includes/core/workspace-overview.md)]

## Creating Workspaces

Workspaces can be created using the <xref:Whim.AddWorkspaceTransform> command. The command returns the ID of the newly created workspace.

```csharp
  Guid stickyWorkspaceId = context.Store.Dispatch(
    new AddWorkspaceTransform(
      Name: "Sticky",
    )
  );
```

The following can be specified when creating a workspace:

- `Name`: The name of the workspace. This defaults to "Workspace n", where `n` is the number of workspaces created so far.
- `CreateLeafLayoutEngines`: A list of layout engines to use for the workspace. This defaults to the layout engines specified in the <xref:Whim.SetCreateLayoutEnginesTransform> command - see [Default Layout Engines](#default-layout-engines) for more.
- `MonitorIndices`: A list of monitor indices to use for the workspace - see [Sticky Workspaces](#sticky-workspaces) for more.

## Default Layout Engines

The <xref:Whim.SetCreateLayoutEnginesTransform> lets you specify the default layout engines for workspaces. For example, the following config sets up three workspaces, and two layout engines:

```csharp
// Set up workspaces.
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform(Name: "Browser")).Value;
Guid ideWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform(Name: "IDE")).Value;
Guid altWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform(Name: "Alt")).Value;

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
        Name: "Alt",
        CreateLeafLayoutEngines: new CreateLeafLayoutEngine[]
        {
            (id) => new FocusLayoutEngine(id)
        }
    )
);
```

The available layout engines are listed in [Layout Engines](./layout-engines.md).

## Sticky Workspaces

Sticky workspaces can only be displayed on specific monitors. To create a sticky workspace, specify the monitor indices when creating the workspace:

```csharp
Guid stickyWorkspaceId = context.Store.Dispatch(
    new AddWorkspaceTransform(
        Name: "Sticky",
        MonitorIndices: new int[] { 0, 1 }
    )
);
```

The workspace will only be displayed on the first and second monitors (0-based index). For more on monitors, see the [Monitors](./monitors.md) page.

## Example Command

```csharp
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform(Name: "Browser")).Value;
Guid ideWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform(Name: "IDE")).Value;

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
