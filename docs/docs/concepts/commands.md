# Commands

Whim stores commands (<xref:Whim.ICommand>) in the <xref:Whim.ICommandManager>. Commands are objects with a unique identifier, title, and executable action. Commands expose easy access to functionality from Whim's core, and loaded plugins.

Command identifiers namespaced to the plugin which defines them. For example, the `whim.core` namespace is reserved for core commands, and `whim.gaps` is used by the `GapsPlugin` to define commands. Identifiers are based on the <xref:Whim.IPlugin.Name> property of the plugin - for example, <xref:Whim.Gaps.GapsPlugin.Name>.

Each plugin can provide commands through the <xref:Whim.IPlugin.PluginCommands> property of the <xref:Whim.IPlugin> interface.

Custom commands are automatically added to the `whim.custom` namespace. For example, the following command minimizes Discord:

```csharp
// Add to the top.
using System.Linq;

void DoConfig(IConfig context)
{
    // ...

    // Create the command.
    context.CommandManager.Add(
        // Automatically namespaced to `whim.custom`.
        identifier: "minimize_discord",
        title: "Minimize Discord",
        callback: () =>
        {
            // Get the first window with the process name "Discord.exe".
            IWindow window = context.WindowManager.FirstOrDefault(w => w.ProcessFileName == "Discord.exe");
            if (window != null)
            {
                // Minimize the window.
                window.ShowMinimized();
                context.WorkspaceManager.ActiveWorkspace.FocusFirstWindow();
            }
        }
    );

    // Create an associated keybind.
    context.KeybindManager.SetKeybind("whim.custom.minimize_discord", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_D));

    // ...
}
```
