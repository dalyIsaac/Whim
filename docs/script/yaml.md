## Layout Engines

The layout engines configuration has a list of layout engines that you can use to customize the layout of your windows. These apply to all workspaces, unless overridden by workspace-specific configuration.

### Layout Engines Example

```yaml
layout_engines:
  entries:
    - type: TreeLayoutEngine
      initial_direction: right

    - type: SliceLayoutEngine
      variant:
        type: row

    - type: SliceLayoutEngine
      variant:
        type: column

    - type: SliceLayoutEngine
      variant:
        type: primary_stack

    - type: SliceLayoutEngine
      variant:
        type: secondary_primary_stack

    - type: SliceLayoutEngine
      variant:
        type: multi_column_stack
        columns: [2, 1, 0]

    - type: SliceLayoutEngine
      variant:
        type: secondary_primary_stack
        primary_capacity: 1
        secondary_capacity: 2

    - type: FocusLayoutEngine
      maximize: false
```

TODO: Add layout engine descriptions
