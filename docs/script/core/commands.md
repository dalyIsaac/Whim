# Commands

Custom commands can be created using the <xref:Whim.ICommandManager>. They are automatically added to the `whim.custom` namespace. For example, the following command closes the current tracked window:

```csharp
// Create the command.
context.CommandManager.Add(
    // Automatically namespaced to `whim.custom`.
    identifier: "close_window",
    title: "Close focused window",
    callback: () =>
    {
        if (context.Store.Pick(Pickers.PickLastFocusedWindow).TryGet(out IWindow window))
        {
            window.Close();
        }
    }
);

// Create an associated keybind.
context.KeybindManager.SetKeybind("whim.custom.close_window", new Keybind([VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LALT], VIRTUAL_KEY.VK_D));
```

The core commands can be found at [Commands](../../configure/core/commands.md).
