# Workspaces

A "workspace" or <xref:Whim.IWorkspace> in Whim is a collection of windows, displayed on a single monitor. The layouts of workspaces are determined by their [layout engines](layout-engines.md). Each workspace has a single active layout engine, and can cycle through different layout engines.

The <xref:Whim.IWorkspaceManager> object has a customizable <xref:Whim.IWorkspaceManager.CreateLayoutEngines> property which provides the default layout engines for workspaces. For example, the following config sets up three workspaces, and two layout engines:

```csharp
// Set up workspaces.
context.WorkspaceManager.Add("Browser");
context.WorkspaceManager.Add("IDE");
context.WorkspaceManager.Add("Alt");

// Set up layout engines.
context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
{
    (id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
    (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
    (id) => SliceLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id),
    (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id),
    (id) => new ColumnLayoutEngine(id)
};
```

It's also possible to customize the layout engines for a specific workspace:

```csharp
context.WorkspaceManager.Add(
    "Alt",
    new CreateLeafLayoutEngine[]
    {
        (id) => new ColumnLayoutEngine(id)
    }
);
```

When Whim exits, it will save the current workspaces and the current positions of each window within them. When Whim is started again, it will attempt to merge the saved workspaces with the workspaces defined in the config.
