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
keybinds:
  entries:
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

## Filters

By default, Whim ignores a built-in list of windows that are known to cause problems with dynamic tiling window manager. Behind the scenes, Whim automatically updates the built-in list of ignored windows based on a subset of the rules from the community-driven [collection of application rules](https://github.com/LGUG2Z/komorebi-application-specific-configuration) managed by komorebi.

### Custom Filtering Behavior

The filters configuration tells Whim to ignore windows that match the specified criteria.

You can filter windows by:

- `window_class`
- `process_file_name`
- `title`
- `title_regex`

### Window Class Filter

For example, to filter out Chromium windows with the class `Chrome_WidgetWin_1`, add the following to your configuration:

```yaml
filters:
  entries:
    - filter_type: window_class
      value: Chrome_WidgetWin_1
```

### Process File Name Filter

For example, to filter out windows with the process file name `explorer.exe`, add the following to your configuration:

```yaml
filters:
  entries:
    - filter_type: process_file_name
      value: explorer.exe
```

### Title Filter

For example, to filter out windows with the title `Untitled - Notepad`, add the following to your configuration:

```yaml
filters:
  entries:
    - filter_type: title
      value: Untitled - Notepad
```

### Title Match Filter

For example, to filter out windows with the title that matches the regex `^Untitled - Notepad$`, add the following to your configuration:

```yaml
filters:
  entries:
    - filter_type: title_regex
      value: ^Untitled - Notepad$
```

## Routers

The routers configuration tells Whim to route windows that match the specified criteria to the first workspace with name `workspace_name`.

### Default Routing Behavior

To customize the default window routing behavior, you can use the `routing_behavior` property. The default routing behavior is `route_to_launched_workspace`.

The available routing behaviors are:

- `route_to_launched_workspace`
- `route_to_active_workspace`
- `route_to_last_tracked_active_workspace`

### Custom Routing Behavior

You can also define custom routing behavior by specifying a list of routing entries. Each routing entry has a `router_type`, `value`, and `workspace_name`.

The available router types are:

- `window_class`
- `process_file_name`
- `title`
- `title_regex`

#### Window Class Router

For example, to route Chromium windows with the class `Chrome_WidgetWin_1` to the workspace `web`, add the following to your configuration:

```yaml
routers:
  entries:
    - router_type: window_class
      value: Chrome_WidgetWin_1
      workspace_name: web
```

#### Process File Name Router

For example, to route windows with the process file name `explorer.exe` to the workspace `file_explorer`, add the following to your configuration:

```yaml
routers:
  entries:
    - router_type: process_file_name
      value: explorer.exe
      workspace_name: file_explorer
```

#### Title Router

For example, to route windows with the title `Untitled - Notepad` to the workspace `notepad`, add the following to your configuration:

```yaml
routers:
  entries:
    - router_type: title
      value: Untitled - Notepad
      workspace_name: notepad
```

#### Title Match Router

For example, to route windows with the title that matches the regex `^Untitled - Notepad$` to the workspace `notepad`, add the following to your configuration:

```yaml
routers:
  entries:
    - router_type: title_regex
      value: ^Untitled - Notepad$
      workspace_name: notepad
```
