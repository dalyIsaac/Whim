# Concepts

## Workspaces

A "workspace" or [`IWorkspace`](api/Whim.IWorkspace.html) in Whim is a collection of windows, displayed on a single monitor. The layouts of workspaces are determined by their [layout engines](#layout-engines). Each workspace has a single active layout engine, and can cycle through different layout engines.

The `WorkspaceManager` object has a customizable `CreateLayoutEngines` property which provides the default layout engines for workspaces. For example, the following config sets up three workspaces, and two layout engines:

```csharp
// Set up workspaces.
context.WorkspaceManager.Add("Browser");
context.WorkspaceManager.Add("IDE");
context.WorkspaceManager.Add("Alt");

// Set up layout engines.
context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
{
    (id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
    (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
    (id) => SliceLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id),
    (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id),
    (id) => new ColumnLayoutEngine(id)
};
```

It's also possible to customize the layout engines for a specific workspace:

```csharp
context.WorkspaceManager.Add(
    "Alt",
    new CreateLeafLayoutEngine[]
    {
        (id) => new ColumnLayoutEngine(id)
    }
);
```

When Whim exits, it will save the current workspaces and the current positions of each window within them. When Whim is started again, it will attempt to merge the saved workspaces with the workspaces defined in the config.

## Layout Engines

A "layout engine" or [`ILayoutEngine`](api/Whim.ILayoutEngine.html) in Whim is responsible for arranging windows in a workspace. Each workspace has a single active layout engine, and can cycle through different layout engines.

There are two different types of layout engines: proxy layout engines, and leaf layout engines. Proxy layout engines wrap other engines, and can be used to modify the behavior of other engines. For example, the [`Gaps` plugin](plugins/gaps.md) will add gaps between windows - normally layout engines won't leave gaps between windows. Leaf layout engines are the lowest level layout engines, and are responsible for actually arranging windows.

To see the available layout engines, see [Layout Engines](layout-engines.md).

## Workspace Manager

The "workspace manager" or [`IWorkspaceManager`](api/Whim.IWorkspaceManager.html) in Whim is responsible for the creation and removal of workspaces. It also manages the active workspace.

## Window Manager

The "window manager" or [`IWindowManager`](api/Whim.IWindowManager.html) is used by Whim to manage [`IWindow`](apis/Whim.IWindow.html)s. It listens to window events from Windows and notifies listeners (Whim core, plugins, etc.).

For example, the `WindowFocused` event is used by the `Whim.FocusIndicator` and `Whim.Bar` plugins to update their indications of the currently focused window.

The `IWindowManager` also exposes an `IFilterManager` called `LocationRestoringFilterManager`. Some applications like to restore their window positions when they start (e.g., Firefox, JetBrains Gateway). As a window manager, this is undesirable. `LocationRestoringFilterManager` listens to `WindowMoved` events for these windows and will force their parent `IWorkspace` to do a layout two seconds after their first `WindowMoved` event, attempting to restore the window to its correct position.

If this doesn't work, dragging a window's edge will force a layout, which should fix the window's position. This is an area which could use further improvement.

## Monitor Manager

The "monitor manager" or [`IMonitorManager`](api/Whim.IMonitorManager.html) in Whim stores the current monitor configuration. It provides methods to get adjacent monitors, and the monitor which contains a given point.

## Butler

The "butler" or [`IButler`](api/Whim.IButler.html) in Whim is responsible for using the [workspace manager](#workspace-manager) and [monitor manager](#monitor-manager) to handle events from the [window manager](#window-manager) to its "butler pantry". The butler also provides access to methods via inheritance from its "butler chores" to manage the movement of windows between workspaces and monitors.

The "butler pantry" or [`IButlerPantry`](api/Whim.IButlerPantry.html) stores the assignment of windows to workspaces, and the assignment of workspaces to monitors.

When [scripting](scripting.md), use `IButler` methods to move windows between workspaces and monitors.

## Commands

Whim stores commands ([`ICommand`](api/Whim.ICommand.html)), which are objects with a unique identifier, title, and executable action. Commands expose easy access to functionality from Whim's core, and loaded plugins.

Command identifiers namespaced to the plugin which defines them. For example, the `whim.core` namespace is reserved for core commands, and `whim.gaps` is used by the `GapsPlugin` to define commands. Identifiers are based on the [`Name`](api/Whim.IPlugin.html) property of the plugin - for example, [`GapsPlugin.Name`](api/Whim.Gaps.GapsPlugin.html#Whim_Gaps_GapsPlugin_Name).

Each plugin can provide commands through the `PluginCommands` property of the [`IPlugin`](api/Whim.IPlugin.html#Whim_IPlugin_PluginCommands) interface.

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
