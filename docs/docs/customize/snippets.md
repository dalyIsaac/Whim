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

## Activate adjacent workspace, skipping over active workspaces

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

The following command can be used to move the active window to a specific workspace, using the workspace ID. The best way to get the workspace ID is to use the return value from the `Add` method on the `WorkspaceManager`.

```csharp
// Once the workspace has been created, it will have this ID.
Guid? browserWorkspaceId = context.WorkspaceManager.Add("Browser workspace");

context.CommandManager.Add("move_window_to_browser_workspace", "Move window to browser workspace", () =>
{
    // This `if` statement protects against workspaces being created with no layout engines.
    if (browserWorkspaceId is Guid workspaceId)
    {
        // This defaults to the active window, but you can also pass in a specific window as the
        // second argument to the transform.
        context.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspaceId));

        // The following call will activate the workspace after moving the window.
        // If the following line is not present, the window will be moved to the target workspace,
        // but the target workspace will not be activated.
        context.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId));
    }
});
```

## Activate a workspace on a specific monitor without changing focus

The following command can be used to activate a workspace on a specific monitor without focusing the workspace you are activating. In this example, I am activating a specific workspace on the 3rd monitor.

```csharp
Guid? browserWorkspaceId = context.WorkspaceManager.Add("Browser workspace");

context.CommandManager.Add(
    identifier: "activate_browser_workspace_on_monitor_3_no_focus",
    title: "Activate browser workspace on monitor 3 no focus",
    callback: () =>
    {
        if (browserWorkspaceId is Guid workspaceId)
        {
            IMonitor? monitor = context.Store.Pick(Pickers.PickMonitorByIndex(2)).ValueOrDefault;
            if (monitor is null)
            {
                return;
            }

            context.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId, monitor.Handle, FocusWorkspaceWindow: false));
        }
    }
);
```
