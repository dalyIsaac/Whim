# Snippets

Potentially useful code snippets.

Each of these snippets (apart from the `using` statements) should be added inside the `DoConfig` function in your `whim.config.csx` file.

## YAML/JSON and C# integration

Add to your `whim.config.csx`:

```csharp
// ...previous references

// NOTE: Replace WHIM_PATH with the path to your Whim installation
#r "WHIM_PATH\plugins\Whim.Yaml\Whim.Yaml.dll"

using System;
using System.Linq;
// ...usings...
using Whim.Yaml;

// ...

void DoConfig(IContext context)
{
    // Make sure to place this at the top of your config
    YamlLoader.Load(context, showErrorWindow: false);

    // Get the loaded plugins.
    CommandPalettePlugin commandPalettePlugin = (CommandPalettePlugin)context.PluginManager.LoadedPlugins.First(p => p.Name == "whim.command_palette");
    TreeLayoutPlugin treeLayoutPlugin = (TreeLayoutPlugin)context.PluginManager.LoadedPlugins.First(p => p.Name == "whim.tree_layout");
    FloatingWindowPlugin floatingWindowPlugin = (FloatingWindowPlugin)context.PluginManager.LoadedPlugins.First(p => p.Name == "whim.floating_window");
}
```

Whim will then look for a `whim.config.yaml` or `whim.config.json` file in the root of your `.whim` directory. If it finds one, it will use that file to configure Whim.

The names of each plugin can be found in the API documentation. For example, `whim.command_palette` is the <xref:Whim.CommandPalette.CommandPalettePlugin.Name> of the Command Palette plugin.

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
        IWindow? result = context
            .Store.Pick(Pickers.PickAllWindows())
            .FirstOrDefault(w => w.ProcessFileName == "Discord.exe");
        if (result is IWindow discord)
        {
            result.ShowMinimized();
            discord.Focus();
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
    callback: () => context.Store.Dispatch(new ActivateAdjacentWorkspaceTransform(Reverse: true, SkipActive: true))
);

// Activate previous workspace, skipping over those that are active on other monitors
context.CommandManager.Add(
    identifier: "activate_next_workspace",
    title: "Activate the next inactive workspace",
    callback: () => context.Store.Dispatch(new ActivateAdjacentWorkspaceTransform(SkipActive: true))
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

The following command can be used to move the active window to a specific workspace, using the workspace ID. The best way to get the workspace ID is to use the return value from the <xref:Whim.AddWorkspaceTransform>.

```csharp
// Once the workspace has been created, it will have this ID.
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Browser")).Value;

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
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Browser")).Value;

context.CommandManager.Add(
    identifier: "activate_browser_workspace_on_monitor_3_no_focus",
    title: "Activate browser workspace on monitor 3 no focus",
    callback: () =>
    {
        if (browserWorkspaceId is Guid workspaceId)
        {
            IMonitor monitor = context.Store.Pick(Pickers.PickMonitorByIndex(2)).Value;
            if (monitor is null)
            {
                return;
            }

            context.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId, monitor.Handle, FocusWorkspaceWindow: false));
        }
    }
);
```
