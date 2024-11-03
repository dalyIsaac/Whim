# Layout Engines

A "layout engine" or <xref:Whim.ILayoutEngine> in Whim is responsible for arranging windows in a workspace. Each workspace has a single active layout engine, and can cycle through different layout engines.

## Available layout engines

| Engine                  | TL;DR                                                                    |
| ----------------------- | ------------------------------------------------------------------------ |
| [`focus`](#focus)       | One window at a time                                                     |
| [`slice`](#slice)       | `Awesome`/`dwm`-style dynamic tiling (primary/stack, multi-column, etc.) |
| [`tree`](#tree)         | `i3`-style dynamic tiling (arbitrary grids)                              |
| [`floating`](#floating) | All windows are free-floating                                            |

## Example Usage

```yaml
layout_engines:
  entries:
    - type: tree
      initial_direction: right

    - type: slice
      variant:
        type: row

    - type: slice
      variant:
        type: column

    - type: slice
      variant:
        type: primary_stack

    - type: slice
      variant:
        type: secondary_primary_stack

    - type: slice
      variant:
        type: multi_column_stack
        columns: [2, 1, 0]

    - type: slice
      variant:
        type: secondary_primary_stack
        primary_capacity: 1
        secondary_capacity: 2

    - type: focus
      maximize: false
```

## `focus`

The `focus` layout engine displays one window at a time.

```yaml
layout_engines:
  entries:
    - type: focus
      maximize: false
```

To show the window in the specified direction, call the `whim.core.focus_window_in_direction.(left|right|up|down)` command - see the [Commands](commands.md) page for more information.

To reorder windows, calling `whim.core.swap_window_in_direction.(left|right|up|down)` will swap the current window with the window in the specified direction. **This will not change the focused window.**

Windows which are not focused are minimized to the taskbar.

![Focus demo](../../images/layout-engines/focus.gif)

## `slice`

The `slice` layout engine, inspired by `Awesome` and `dwm`, arranges windows in a configurable grid according to deterministic algorithm.

The YAML/JSON configuration supports built-in layouts, such as primary/stack, multi-column, and secondary primary stack. Arbitrary layouts can be created by nesting areas in the C# configuration - see [Defining different `SliceLayouts`](../../script/core/layout-engines.md#defining-different-slicelayouts)

Commands for the `slice` layout engine can be found on the [Slice Layout](../plugins/slice-layout.md#commands) plugin page.

### Layout Variants

### Column Layout

- **Description**: Creates a column layout, where windows are stacked vertically.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: slice
        variant: column
  ```

![Column layout demo](../../images/layout-engines/column.gif)

#### Row Layout

- **Description**: Creates a row layout, where windows are stacked horizontally.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: slice
        variant: row
  ```

![Row layout demo](../../images/layout-engines/row.gif)

#### Primary Stack Layout

- **Description**: Creates a primary stack layout, where the first window takes up half the screen, and the remaining windows are stacked vertically on the other half.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: slice
        variant: primary_stack
  ```

![Primary stack layout demo](../../images/layout-engines/primary-stack.gif)

#### Multi-Column Stack Layout

- **Description**: Creates a multi-column layout with the given number of windows in each column. `[2, 1, 0]` will create a layout with 3 columns, where the first column has 2 windows, the second column has 1 window, and the third column has infinite windows.
- **Properties**:
  - `columns`: An array of integers specifying the number of windows in each column.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: slice
        variant: multi_column_stack
        columns: [2, 1, 0]
  ```

![Multi-column stack layout demo](../../images/layout-engines/multi-column-stack.gif)

#### Secondary Primary Stack Layout

- **Description**: Creates a three-column layout, where the primary column is in the middle, the secondary column is on the left, and the overflow column is on the right. The middle column takes up 50% of the screen, and the left and right columns take up 25%.
- **Properties**:
  - `primary_capacity`: The number of rows in the primary column. This must be a non-negative integer. Default is 1.
  - `secondary_capacity`: The number of rows in the secondary column. This must be a non-negative integer. Default is 2.
- **Example**:

  ```yaml
  layout_engines:
    entries:
      - type: slice
        variant: secondary_primary_stack
        primary_capacity: 1
        secondary_capacity: 2
  ```

![Secondary primary stack demo](../../images/layout-engines/secondary-primary-stack.gif)

## `tree`

The `tree` layout engine is a layout that allows users to create arbitrary grid layouts, similar to `i3`. Unlike the [`slice`](#slice) layout engine, windows can can be added in any location at runtime.

The `tree` supports an `initial_direction` property, which specifies the direction of the first split. The available directions are `left`, `right`, `up`, and `down`.

```yaml
layout_engines:
  entries:
    - type: tree
      initial_direction: right
```

![Tree demo](../../images/layout-engines/tree.gif)

Commands for the `tree` layout engine can be found on the [Tree Layout](../plugins/tree-layout.md#commands) plugin page.

## `floating`

The `floating` layout engine is a layout that has all windows being free-floating. To have specific windows float within a different layout, see the [Floating Window Plugin](../plugins/floating-window.md).

```yaml
layout_engines:
  entries:
    - type: floating
```

![Floating layout demo](../../images/layout-engines/floating.gif)
