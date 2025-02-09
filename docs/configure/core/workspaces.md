# Workspaces

[!INCLUDE[Workspace overview](../../_includes/core/workspace-overview.md)]

## Configuration

The `workspaces` property is a list of workspaces that can be displayed on a monitor. Each workspace has a name and a list of layout engines. The layout engines determine how windows are displayed on the workspace. If no layout engines are provided, the workspace will default to the layout engines defined in [Layout Engines](layout-engines.md).

```yaml
workspaces:
  entries:
    - name: Browser
      layout_engines:
        entries:
          - type: SliceLayoutEngine
            variant:
              type: column
          - type: SliceLayoutEngine
            variant:
              type: row
          - type: TreeLayoutEngine

    - name: IDE
    - name: Chat
    - name: Spotify
    - name: Other
```

## Sticky Workspaces

Sticky workspaces can only be displayed on specific monitors. To create a sticky workspace, specify the monitor indices when creating the workspace:

```yaml
workspaces:
  entries:
    - name: Browser
      monitors: [0, 1]
```

Here, the workspace can only be displayed on the first and second monitors (0-based index). For more on the ordering of monitors monitors, see the [Monitors](monitors.md) page.
