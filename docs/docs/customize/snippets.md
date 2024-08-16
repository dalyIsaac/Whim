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

The following commands can be useful on multi-monitor setups. When bound to keybinds, these can be used to cycle through the list of workspaces, skipping over those that are active on other monitors to avoid accidental swapping.

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

## Move a window to a specific monitor

The following command can be used to move the active window to a specific monitor. The index of the monitor is zero-based, so the primary monitor is index 0, the second monitor is index 1, and so on.

```csharp
context.CommandManager.Add("move_window_to_monitor_2", "Move window to monitor 2", () =>
{
    context.Store.Dispatch(new MoveWindowToMonitorIndexTransform(1));
});
```

## Move a window to a specific workspace

The following command can be used to move the active window to a specific workspace. The index of the workspace is zero-based.

```csharp
Guid? browserWorkspaceId = context.WorkspaceManager.Add("Browser workspace");
context.CommandManager.Add("move_window_to_browser_workspace", "Move window to browser workspace", () =>
{
    // This `if` statement protects against workspaces being created with no layout engines.
    if (browserWorkspaceId is Guid workspaceId)
    {
        // This defaults to the active window, but you can also pass in a specific window as the second argument to the transform.
        context.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspaceId));
    }
});
```
