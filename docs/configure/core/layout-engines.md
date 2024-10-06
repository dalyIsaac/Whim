# Layout Engines

A "layout engine" or <xref:Whim.ILayoutEngine> in Whim is responsible for arranging windows in a workspace. Each workspace has a single active layout engine, and can cycle through different layout engines.

## Available layout engines

| Engine                                    | TL;DR                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------ |
| [`FocusLayoutEngine`](#focuslayoutengine) | One window at a time                                                     |
| [`SliceLayoutEngine`](#slicelayoutengine) | `Awesome`/`dwm`-style dynamic tiling (primary/stack, multi-column, etc.) |
| [`TreeLayoutEngine`](#treelayoutengine)   | `i3`-style dynamic tiling (arbitrary grids)                              |

## Example Usage

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

## `FocusLayoutEngine`

The `FocusLayoutEngine` is a layout engine that displays one window at a time.

```yaml
layout_engines:
  entries:
    - type: FocusLayoutEngine
      maximize: false
```

To show the window in the specified direction, call the `whim.core.focus_window_in_direction.(left|right|up|down)` command - see the [Commands](commands.md) page for more information.

To reorder windows, calling `whim.core.swap_window_in_direction.(left|right|up|down)` will swap the current window with the window in the specified direction. **This will not change the focused window.**

Windows which are not focused are minimized to the taskbar.

![FocusLayoutEngine demo](../../images/focus-layout-demo.gif)

## `SliceLayoutEngine`

`SliceLayoutEngine` is an `Awesome`/`dwm`-style layout engine, which arranges following a deterministic algorithm filling a grid configured in the config file.

The YAML/JSON configuration supports built-in layouts, such as primary/stack, multi-column, and secondary primary stack. Arbitrary layouts can be created by nesting areas in the C# configuration - see [Defining different `SliceLayouts`](../../script/core/layout-engines.md#defining-different-slicelayouts)

### Layout Variants

### Column Layout

- **Description**: Creates a column layout, where windows are stacked vertically.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: SliceLayoutEngine
        variant: column
  ```

#### Row Layout

- **Description**: Creates a row layout, where windows are stacked horizontally.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: SliceLayoutEngine
        variant: row
  ```

#### Primary Stack Layout

- **Description**: Creates a primary stack layout, where the first window takes up half the screen, and the remaining windows are stacked vertically on the other half.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: SliceLayoutEngine
        variant: primary_stack
  ```

#### Multi-Column Stack Layout

- **Description**: Creates a multi-column layout with the given number of windows in each column. `[2, 1, 0]` will create a layout with 3 columns, where the first column has 2 windows, the second column has 1 window, and the third column has infinite windows.
- **Properties**:
  - `columns`: An array of integers specifying the number of windows in each column.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: SliceLayoutEngine
        variant: multi_column_stack
        columns: [2, 1, 0]
  ```

#### Secondary Primary Stack Layout

- **Description**: Creates a three-column layout, where the primary column is in the middle, the secondary column is on the left, and the overflow column is on the right. The middle column takes up 50% of the screen, and the left and right columns take up 25%.
- **Properties**:
  - `primary_capacity`: The number of rows in the primary column. This must be a non-negative integer. Default is 1.
  - `secondary_capacity`: The number of rows in the secondary column. This must be a non-negative integer. Default is 2.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: SliceLayoutEngine
        variant: secondary_primary_stack
        primary_capacity: 1
        secondary_capacity: 2
  ```

![SliceLayoutEngine demo](../../images/slice-layout-demo.gif)

## `TreeLayoutEngine`

<xref:Whim.TreeLayout.TreeLayoutEngine> is a layout that allows users to create arbitrary grid layouts, similar to `i3`. Unlike [`SliceLayoutEngine`](#slicelayoutengine), windows can can be added in any location at runtime.

The `TreeLayoutEngine` supports an `initial_direction` property, which specifies the direction of the first split. The available directions are `left`, `right`, `up`, and `down`.

```yaml
layout_engines:
  entries:
    - type: TreeLayoutEngine
      initial_direction: right
```

<!-- TODO: Commands for the TreeLayoutEngine -->

![TreeLayoutEngine demo](../../images/tree-layout-demo.gif)

## `FloatingLayoutEngine`

> [!WARNING]
> This is not yet supported by the YAML/JSON configuration.

<xref:Whim.FloatingWindow.FloatingLayoutEngine> is a layout that has all windows being free-floating. To have specific windows float within a different layout, see the [Floating Window Plugin](../plugins/floating-window.md).

![FloatingLayoutEngine demo](../../images/floating-layout-demo.gif)
