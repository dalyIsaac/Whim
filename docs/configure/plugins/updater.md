# Updater Plugin

The `UpdaterPlugin` adds the ability to notify the user when a new version of Whim is available.

```yaml
plugins:
  updater:
    is_enabled: true
    release_channel: alpha
    update_frequency: weekly
```

## Configuration

| Property           | Description                                                                                                 |
| ------------------ | ----------------------------------------------------------------------------------------------------------- |
| `is_enabled`       | Whether the plugin is enabled. Default is `true`.                                                           |
| `release_channel`  | The release channel to use. Options are `alpha`, `beta`, and `stable`. Default is `alpha`.                  |
| `update_frequency` | How often to check for updates. Options are `never`, `daily`, `weekly`, and `monthly`. Default is `weekly`. |

[!INCLUDE [Commands](../../_includes/plugins/updater.md)]
