# Command Palette Plugin

The `CommandPalettePlugin` adds the command palette to Whim.

![Command palette demo](../../images/command-palette-demo.gif)

```yaml
plugins:
  command_palette:
    is_enabled: true

    max_height_percent: 40
    max_width_pixels: 800
    y_position_percent: 20
```

## Configuration

| Property             | Description                                                                      |
| -------------------- | -------------------------------------------------------------------------------- |
| `is_enabled`         | Whether the plugin is enabled. Defaults to `true`.                               |
| `max_height_percent` | The maximum height of the command palette as a percentage of the monitor height. |
| `max_width_pixels`   | The maximum width of the command palette in pixels.                              |
| `y_position_percent` | The y position of the command palette as a percentage of the monitor height.     |

[!INCLUDE [Commands](../../_common/plugins/command-palette.md)]

## Tree Layout

👷🏗️🚧