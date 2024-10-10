# Bar Plugin

The `BarPlugin` adds a configurable bar at the top of each monitor.

![Bar demo](../../images/bar-demo.png)

```yaml
plugins:
  bar:
    left_components:
      entries:
        - type: workspace_widget

    center_components:
      entries:
        - type: focused_window_widget
          shorten_title: true

    right_components:
      entries:
        - type: battery_widget
        - type: active_layout_widget
        - type: date_time_widget
          format: HH:mm:ss, dd MMM yyyy
        - type: tree_layout_widget
```

## Configuration

| Property            | Description                                                                     |
| ------------------- | ------------------------------------------------------------------------------- |
| `is_enabled`        | Whether the plugin is enabled                                                   |
| `height`            | The height of the bar in pixels.                                                |
| `backdrop`          | The backdrop to use for the bar - see [Backdrops](../core/styling.md#backdrops) |
| `left_components`   | The widgets to display on the left side of the bar.                             |
| `center_components` | The widgets to display in the center of the bar.                                |
| `right_components`  | The widgets to display on the right side of the bar.                            |

## Components

The `left_components`, `center_components`, and `right_components` properties have lists of components under the `entries` key. Each component has a `type` key that specifies the type of widget to use. The following widgets are available:

- [Configuration](#configuration)
- [Components](#components)
- [Widgets](#widgets)
  - [Active Layout Widget](#active-layout-widget)
  - [Battery Widget](#battery-widget)
  - [Date Time Widget](#date-time-widget)
  - [Focused Window Widget](#focused-window-widget)
  - [Workspace Widget](#workspace-widget)
  - [Tree Layout Widget](#tree-layout-widget)

## Widgets

### Active Layout Widget

The `ActiveLayoutWidget` displays the name of the current layout.

### Battery Widget

The `BatteryWidget` displays the battery percentage and status.

### Date Time Widget

The `DateTimeWidget` displays the current date and time.

| Property   | Description                                                                                                                                                                                      |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `interval` | The interval in milliseconds to update the date and time.                                                                                                                                        |
| `format`   | The format to display the date and time in. For more, see [Custom date and time format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) |

### Focused Window Widget

The `FocusedWindowWidget` displays the title of the focused window.

| Property        | Description                                 |
| --------------- | ------------------------------------------- |
| `shorten_title` | Whether to shorten the title of the window. |

### Workspace Widget

The `WorkspaceWidget` displays the name of the current workspace.

### Tree Layout Widget

The `TreeLayoutWidget` displays the direction to add windows in the tree layout engine on the current workspace.
