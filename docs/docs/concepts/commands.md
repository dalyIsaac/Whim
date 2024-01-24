# Commands

Whim stores commands (<xref:Whim.ICommand>) in the <xref:Whim.ICommandManager>. Commands are objects with a unique identifier, title, and executable action. Commands expose easy access to functionality from Whim's core, and loaded plugins.

Command identifiers namespaced to the plugin which defines them. For example, the `whim.core` namespace is reserved for core commands (see [Core Commands](../core-commands.md)), and `whim.gaps` is used by the `GapsPlugin` to define commands. Identifiers are based on the <xref:Whim.IPlugin.Name> property of the plugin - for example, <xref:Whim.Gaps.GapsPlugin.Name>.

Each plugin can provide commands through the <xref:Whim.IPlugin.PluginCommands> property of the <xref:Whim.IPlugin> interface.

Custom commands are automatically added to the `whim.custom` namespace. For example, the following command closes the current tracked window:

```csharp
// Create the command.
context.CommandManager.Add(
    // Automatically namespaced to `whim.custom`.
    identifier: "close_window",
    title: "Close focused window",
    callback: () => context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Close()
);

// Create an associated keybind.
context.KeybindManager.SetKeybind("whim.custom.close_window", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_D));
```
