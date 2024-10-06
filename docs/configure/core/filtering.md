# Filters

By default, Whim ignores a built-in list of windows that are known to cause problems with dynamic tiling window manager. Behind the scenes, Whim automatically updates the built-in list of ignored windows based on a subset of the rules from the community-driven [collection of application rules](https://github.com/LGUG2Z/komorebi-application-specific-configuration) managed by komorebi.

## Custom Filtering Behavior

The filters configuration tells Whim to ignore windows that match the specified criteria.

You can filter windows by:

- `window_class`
- `process_file_name`
- `title`
- `title_regex`

## Window Class Filter

For example, to filter out Chromium windows with the class `Chrome_WidgetWin_1`, add the following to your configuration:

```yaml
filters:
  entries:
    - type: window_class
      value: Chrome_WidgetWin_1
```

## Process File Name Filter

For example, to filter out windows with the process file name `explorer.exe`, add the following to your configuration:

```yaml
filters:
  entries:
    - type: process_file_name
      value: explorer.exe
```

## Title Filter

For example, to filter out windows with the title `Untitled - Notepad`, add the following to your configuration:

```yaml
filters:
  entries:
    - type: title
      value: Untitled - Notepad
```

## Title Match Filter

For example, to filter out windows with the title that matches the regex `^Untitled - Notepad$`, add the following to your configuration:

```yaml
filters:
  entries:
    - type: title_regex
      value: ^Untitled - Notepad$
```
