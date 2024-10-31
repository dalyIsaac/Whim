# Styling

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

### Configuration

| Property               | Description                                                                                                                                                                  |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `type`                 | The type of backdrop to use.                                                                                                                                                 |
| `always_show_backdrop` | By default, WinUI will disable the backdrop when the window loses focus. Whim overrides this setting. Set this to false to disable the backdrop when the window loses focus. |

## XAML Styling

Loading XAML styles via YAML/JSON is being tracked in [this GitHub issue](https://github.com/dalyIsaac/Whim/issues/1064). In the meantime, it is available [via C# scripting](../../script/core/styling.md).
