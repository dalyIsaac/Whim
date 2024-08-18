# Workspaces

A "workspace" or <xref:Whim.IWorkspace> in Whim is a collection of windows displayed on a single monitor. The layouts of workspaces are determined by their [layout engines](layout-engines.md). Each workspace has a single active layout engine, and can cycle through different layout engines.

The <xref:Whim.IWorkspaceManager> object has a customizable <xref:Whim.IWorkspaceManager.CreateLayoutEngines> property which provides the default layout engines for workspaces. For example, the following config sets up three workspaces, and two layout engines:

```csharp
// Set up workspaces.
context.WorkspaceManager.Add("Browser");
context.WorkspaceManager.Add("IDE");
context.WorkspaceManager.Add("Alt");

// Set up layout engines.
context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
{
    (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
    (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id)
};
```

It's also possible to customize the layout engines for a specific workspace:

```csharp
context.WorkspaceManager.Add(
    "Alt",
    new CreateLeafLayoutEngine[]
    {
        (id) => new FocusLayoutEngine(id)
    }
);
```

The available layout engines are listed in [Layout Engines](./layout-engines.md).

If no name is provided, the name will default to `Workspace {workspaces.Count + 1}.`

When Whim exits, it will save the current workspaces and the current positions of each window within them. When Whim is started again, it will attempt to merge the saved workspaces with the workspaces defined in the config.

> [!NOTE]
> Whim does not support Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors.
