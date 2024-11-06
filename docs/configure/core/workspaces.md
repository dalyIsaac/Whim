# Workspaces

[!INCLUDE[Workspace overview](../../_includes/core/workspace-overview.md)]

## Configuration

The `workspaces` property is a list of workspaces that can be displayed on a monitor. Each workspace has a name and a list of layout engines. The layout engines determine how windows are displayed on the workspace. If no layout engines are provided, the workspace will default to the layout engines defined in [Layout Engines](layout-engines.md).

```yaml
workspaces:
  entries:
    - name: Browser
      layout_engines:
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

Sticky workspaces are being worked on in [this GitHub issue](https://github.com/dalyIsaac/Whim/issues/660).

👷🏗️🚧
