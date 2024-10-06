# Deprecated

The following items will be deprecated and later be removed in a future release.

## Butler

> [!NOTE]
> The `Butler` has been replaced by the [`Store`](./store.md).

The "butler" or <xref:Whim.IButler> in Whim was responsible for using the <xref:Whim.IWorkspaceManager> and <xref:Whim.IMonitorManager> to handle events from the [window manager](#window-manager) to its "butler pantry". The butler also provided access to methods via inheritance from its "butler chores" to manage the movement of windows between workspaces and monitors.

The "butler pantry" or <xref:Whim.IButlerPantry> stored the assignment of windows to workspaces, and the assignment of workspaces to monitors.

When scripting, use `IButler` methods to move windows between workspaces and monitors.

## Window Manager

> [!NOTE]
> The `WindowManager` has been replaced by the [`Store`](./store.md).

The "window manager" or <xref:Whim.IWindowManager> was used by Whim to manage <xref:Whim.IWindow>s. It listens to window events from Windows and notifies listeners (Whim core, plugins, etc.).

For example, the `WindowFocused` event is used by the `Whim.FocusIndicator` and `Whim.Bar` plugins to update their indications of the currently focused window.

## Workspace Manager

> [!NOTE]
> The `WorkspaceManager` has been replaced by the [`Store`](./store.md).

The "workspace manager" or <xref:Whim.IWorkspaceManager> was used by Whim to manage <xref:Whim.IWorkspace>s. It listened to workspace events from Windows and notifies listeners (Whim core, plugins, etc.).

## Monitor Manager

> [!NOTE]
> The `MonitorManager` has been replaced by the [`Store`](./store.md).

Whim supports multiple monitors via the <xref:Whim.IMonitorManager>, which stores the current monitor configuration. `IMonitorManager` provides various methods including the ability get adjacent monitors, and the monitor which contains a given point.

Each <xref:Whim.IMonitor> contains properties like its scale factor.
