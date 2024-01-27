# Keybinds

A [command](commands.md) can be bound to a single "keybind" <xref:Whim.IKeybind>. However, **each keybind can trigger multiple commands.**

The <xref:Whim.IKeybind> interface contains a number of constants with common <xref:Whim.KeyModifiers>. When creating a custom <xref:Whim.Keybind>, you can use these constants or a custom combination of <xref:Whim.KeyModifiers>.

The available keys can be found in the <xref:Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY> enum.
Keybinds can be overridden and removed in the config. For example:

```csharp
// Override the default keybind for showing/hiding the command palette.
context.KeybindManager.SetKeybind("whim.command_palette.toggle", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_P));

// Remove the default keybind for closing the current workspace.
context.KeybindManager.Remove("whim.core.close_current_workspace");

// Remove all keybinds - start from scratch.
context.KeybindManager.Clear();
```

> [!WARNING]
> When overriding keybinds for plugins, make sure to set the keybind **after** calling `context.PluginManager.AddPlugin(plugin)`.
>
> Otherwise, `PluginManager.AddPlugin` will set the default keybinds, overriding custom keybinds set before the plugin is added.

To treat key modifiers like `LWin` and `RWin` the same, see <xref:Whim.IKeybindManager.UnifyKeyModifiers>.
