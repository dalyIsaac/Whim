# YAML Configuration (WIP)

Whim is in the process of adding YAML configuration support - this is being tracked in [#1009](https://github.com/dalyIsaac/Whim/issues/1009). This has been implemented in the `Whim.Yaml` plugin.

## Setup

Whim looks for a `whim.config.yaml` or `whim.config.json` file in the root of your `.whim` directory. If it finds one, it will use that file to configure Whim.

Add to your `whim.config.csx`:

```csharp
// ...previous references

// NOTE: Replace WHIM_PATH with the path to your Whim installation
#r "WHIM_PATH\plugins\Whim.Yaml\Whim.Yaml.dll"

using System;
// ...usings...
using Whim.Yaml;

// ...

void DoConfig(IContext context)
{
    // Make sure to place this at the top of your config
    YamlLoader.Load(context);
}
```

## Schema

The schema for the YAML and JSON configuration is available [here](https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json).

To use the schema in your YAML file, add the following line at the top of your file:

```yaml
# yaml-language-server: $schema=https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json
```

To use the schema in your JSON file, add the following line at the top of your file:

```json
{
  "$schema": "https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json",
  ...
}
```

## Keybinds

The keybinds configuration has a list of bindings that you can use to bind commands to key combinations.

### Bindings

A key modifier is a key that is pressed in combination with another key to perform a specific action. The `bindings` property is a list of keybinds that map a command to a key combination.

Key modifiers can be any of the following:

- `Ctrl`
- `Control`
- `LCtrl`
- `LControl`
- `RCtrl`
- `RControl`
- `Shift`
- `LShift`
- `RShift`
- `Alt`
- `LAlt`
- `RAlt`
- `Win`
- `LWin`
- `RWin`

The associated key for each modifier can be any of the <xref:Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY>s.

### Unify Key Modifiers

To treat key modifiers like `LWin` and `RWin` the same, set `unify_key_modifiers` to `true`.

### Keybinds Example

```yaml
# yaml-language-server: $schema=https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json
keybinds:
  bindings:
    - command: whim.core.focus_next_monitor
      keybind: LCtrl+LShift+LAlt+K

    - command: whim.core.focus_previous_monitor
      keybind: LCtrl+LShift+LAlt+J

    - command: whim.custom.next_layout_engine
      keybind: LCtrl+LShift+LAlt+L

    - command: whim.core.cycle_layout_engine.next
      keybind: LCtrl+LShift+LAlt+L

    - command: whim.core.cycle_layout_engine.previous
      keybind: LCtrl+LShift+LAlt+Win+L

    - command: whim.command_palette.find_focus_window
      keybind: Win+LCtrl+F

    - command: whim.core.exit_whim
      keybind: Win+LCtrl+Q

  unify_key_modifiers: true
```
