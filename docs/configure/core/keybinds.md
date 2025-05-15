# Keybinds

The keybinds configuration has a list of bindings that you can use to bind commands to key combinations.

## Bindings

A key modifier is a key that is pressed in combination with another key to perform a specific action. The `bindings` property is a list of keybinds that map a command to a key combination.

Key modifiers are typically one of the following:

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

Keybinds can also be any other key, though it's recommended to use keys which aren't typically used for other purposes.

The associated key for each modifier can be any of the <xref:Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY>s, without the `VK*` prefix.

## Commands

A command is a string that represents a command that can be executed by Whim. The command can be a built-in command, a plugin command, or a custom command. For more, see the [Commands](commands.md) page.

## Unify Key Modifiers

To treat key modifiers like `LWin` and `RWin` the same, set `unify_key_modifiers` to `true`.

## Keybinds Example

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
