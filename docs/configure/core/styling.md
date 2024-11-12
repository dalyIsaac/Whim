# Styling

## XAML Styling

XAML resources can be loaded via the `styles` key in the YAML/JSON configuration. Each path should be a path to a XAML file. Paths must be absolute (e.g. `C:\Users\user\.whim\resources\styles.xaml`) or relative to the `.whim` directory (e.g. `resources\styles.xaml`).

For example:

```yaml
styles:
  user_dictionaries:
    - resources/styles.xaml
```

[!INCLUDE [Styling](../../_includes/core/styling.md)]

## Backdrops

Different Whim windows can support custom backdrops. They will generally be associated with a `backdrop` key in the YAML/JSON configuration. The following backdrops are available:

- `none`: No backdrop
- `acrylic`: An [acrylic backdrop](https://docs.microsoft.com/en-us/windows/apps/design/style/acrylic)
- `acrylic_thin`: A more transparent Acrylic backdrop - based on the Acrylic backdrop

| Type           | Description                                                                                                             | WinUI Documentation                                                                    |
| -------------- | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| `none`         | No backdrop                                                                                                             | N/A                                                                                    |
| `acrylic`      | A translucent texture that blurs the content behind it.                                                                 | [Acrylic material](https://docs.microsoft.com/en-us/windows/apps/design/style/acrylic) |
| `acrylic_thin` | A more transparent version of the Acrylic backdrop.                                                                     | N/A                                                                                    |
| `mica`         | An opaque, dynamic material that incorpoates theme and the desktop wallpaper. Mica has better performance than Acrylic. | [Mica material](https://learn.microsoft.com/en-us/windows/apps/design/style/mica)      |
| `mica_alt`     | A variant of Mica with stronger tinting of the user's background color.                                                 | [Mica alt material](https://learn.microsoft.com/en-us/windows/apps/design/style/mica)  |

### Backdrops Configuration

| Property               | Description                                                                                                                                                                  |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `type`                 | The type of backdrop to use.                                                                                                                                                 |
| `always_show_backdrop` | By default, WinUI will disable the backdrop when the window loses focus. Whim overrides this setting. Set this to false to disable the backdrop when the window loses focus. |

```yaml
backdrop:
  type: acrylic
  always_show_backdrop: true
```
