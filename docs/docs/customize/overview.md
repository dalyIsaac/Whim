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

## YAML Example

```yaml
# yaml-language-server: $schema=https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json
keybinds:
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
```

## JSON Example

```json
{
  "$schema": "https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json",

  "keybinds": [
    {
      "command": "whim.core.focus_next_monitor",
      "keybind": "LCtrl+LShift+LAlt+K"
    },
    {
      "command": "whim.core.focus_previous_monitor",
      "keybind": "LCtrl+LShift+LAlt+J"
    },
    {
      "command": "whim.custom.next_layout_engine",
      "keybind": "LCtrl+LShift+LAlt+L"
    },
    {
      "command": "whim.core.cycle_layout_engine.next",
      "keybind": "LCtrl+LShift+LAlt+L"
    },
    {
      "command": "whim.core.cycle_layout_engine.previous",
      "keybind": "LCtrl+LShift+LAlt+Win+L"
    },
    {
      "command": "whim.command_palette.find_focus_window",
      "keybind": "Win+LCtrl+F"
    },
    {
      "command": "whim.core.exit_whim",
      "keybind": "Win+LCtrl+Q"
    }
  ]
}
```
