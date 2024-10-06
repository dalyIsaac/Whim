# Gaps Plugin

The `GapsPlugin` adds the config and commands to add gaps between each of the windows in the layout.

![Gaps plugin demo](../../images/gaps-demo.png)

```yaml
plugins:
  gaps:
    is_enabled: true
    outer_gap: 10
    inner_gap: 10
    default_outer_delta: 2
    default_inner_delta: 2
```

## Configuration

| Property              | Description                                                                            |
| --------------------- | -------------------------------------------------------------------------------------- |
| `is_enabled`          | Whether the plugin is enabled                                                          |
| `outer_gap`           | The gap between the parent layout engine and the area where windows are placed         |
| `inner_gap`           | The gap between windows                                                                |
| `default_outer_delta` | The default outer gap used by commands `gaps.outer.increase` and `gaps.outer.decrease` |
| `default_inner_delta` | The default inner gap used by commands `gaps.inner.increase` and `gaps.inner.decrease` |

[!INCLUDE [Commands](../../_includes/plugins/gaps.md)]
