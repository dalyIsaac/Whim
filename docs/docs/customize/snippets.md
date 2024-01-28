# Snippets

Potentially useful code snippets.

## Minimize a specific window

```csharp
// Add by the other `using` statements.
using System.Linq;

// Minimize Discord.
context.CommandManager.Add(
    identifier: "minimize_discord",
    title: "Minimize Discord",
    callback: () =>
    {
        // Get the first window with the process name "Discord.exe".
        IWindow window = context.WindowManager.FirstOrDefault(w => w.ProcessFileName == "Discord.exe");
        if (window != null)
        {
            window.ShowMinimized();
            context.WorkspaceManager.ActiveWorkspace.FocusFirstWindow();
        }
    }
);
```

## Skip over active workspaces

```csharp
// Activate next workspace, skipping over those that are active on other monitors
context.CommandManager.Add(
    identifier: "activate_previous_workspace",
    title: "Activate the previous inactive workspace",
    callback: () => context.WorkspaceManager.ActivateAdjacent(reverse: true, skipActive: true)
);

// Activate previous workspace, skipping over those that are active on other monitors
context.CommandManager.Add(
    identifier: "activate_next_workspace",
    title: "Activate the next inactive workspace",
    callback: () => context.WorkspaceManager.ActivateAdjacent(skipActive: true)
);
```
