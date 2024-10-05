# Routers

The routers configuration tells Whim to route windows that match the specified criteria to the first workspace with name `workspace_name`.

## Default Routing Behavior

To customize the default window routing behavior, you can use the `routing_behavior` property. The default routing behavior is `route_to_launched_workspace`.

The available routing behaviors are:

- `route_to_launched_workspace`
- `route_to_active_workspace`
- `route_to_last_tracked_active_workspace`

## Custom Routing Behavior

You can also define custom routing behavior by specifying a list of routing entries. Each routing entry has a `type`, `value`, and `workspace_name`.

The available router types are:

- `window_class`
- `process_file_name`
- `title`
- `title_regex`

## Window Class Router

For example, to route Chromium windows with the class `Chrome_WidgetWin_1` to the workspace `web`, add the following to your configuration:

```yaml
routers:
  entries:
    - type: window_class
      value: Chrome_WidgetWin_1
      workspace_name: web
```

## Process File Name Router

For example, to route windows with the process file name `explorer.exe` to the workspace `file_explorer`, add the following to your configuration:

```yaml
routers:
  entries:
    - type: process_file_name
      value: explorer.exe
      workspace_name: file_explorer
```

## Title Router

For example, to route windows with the title `Untitled - Notepad` to the workspace `notepad`, add the following to your configuration:

```yaml
routers:
  entries:
    - type: title
      value: Untitled - Notepad
      workspace_name: notepad
```

## Title Match Router

For example, to route windows with the title that matches the regex `^Untitled - Notepad$` to the workspace `notepad`, add the following to your configuration:

```yaml
routers:
  entries:
    - type: title_regex
      value: ^Untitled - Notepad$
      workspace_name: notepad
```
