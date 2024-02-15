# Keybinds

Keybinds are managed using the <xref:Whim.IKeybindManager>, which can be used to create new keybinds and to remove or overwrite existing keybinds.

## Default Keybinds

See the listing of [core commands](commands.md#core-commands) for a summary of default keybinds. Some plugins also come with additional default keybinds, which are documented on the individual plugin pages.

## Creating Keybinds

New keybindings are created by binding a [command](commands.md) identifier to a "Keybind" (<xref:Whim.Keybind>). For instance, the following binds `whim.core.cycle_layout_engine.next` to <kbd>Alt</kbd> + <kbd>SPACE</kbd>.

```csharp
context.KeybindManager.SetKeybind("whim.core.cycle_layout_engine.next", new Keybind(KeyModifiers.LAlt, VIRTUAL_KEY.VK_SPACE));
```

Keybinds have a `Modifiers` and `Key` property. The available modifiers and keys can be found in the <xref:Whim.KeyModifiers> and <xref:Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY> enums. Modifiers can be combined using the bitwise OR operator. For instance, the following creates a new <kbd>Alt</kbd> + <kbd>Shift</kbd> modifier, which can be used to create Keybinds:

```csharp
KeyModifiers AltShift = KeyModifiers.LAlt | KeyModifiers.LShift;
```

A number of common modifiers combinations are also accessible from the <xref:Whim.IKeybind> interface.

To treat key modifiers like `LWin` and `RWin` the same, see <xref:Whim.IKeybindManager.UnifyKeyModifiers>.

> [!NOTE]
> Each _command_ can only be bound to a single keybind - subsequent bindings to the same command will overwrite earlier ones. However, each _keybind_ can be assigned to multiple commands. If more than one command is bound to the same Keybind, they will be triggered in the order of their assignment.

## Overwriting Keybinds

Default bindings can be overwritten by simply rebinding the corresponding _command_ as described above.

> [!WARNING]
> When overriding keybinds for plugins, make sure to set the keybind **after** calling `context.PluginManager.AddPlugin(plugin)`.
>
> Otherwise, `PluginManager.AddPlugin` will set the default keybinds, overriding custom keybinds set before the plugin is added.

## Removing Keybinds

Keybinds can be removed for individual commands or entirely:

```csharp
// Remove the default keybind for closing the current workspace.
context.KeybindManager.Remove("whim.core.close_current_workspace");

// Remove all keybinds - start from scratch.
context.KeybindManager.Clear();
```
