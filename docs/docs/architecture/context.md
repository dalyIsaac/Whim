# `IContext`

The <xref:Whim.IContext> is the core of Whim. It consists of managers which contain and control Whim's state and functionality.

The <xref:Whim.IContext> consists of:

- the <xref:Whim.IButler>
- managers which contain and control Whim's state and functionality
- events related to the `IContext`
- the <xref:Whim.IContext.UncaughtExceptionHandling> setting
- the <xref:Whim.Logger>
- the <xref:Whim.INativeManager> with some [CsWin32](./native-apis.md) APIs available for use in plugins

The <xref:Whim.IButler> uses the various managers to handle events from Windows and the user to update the mapping of <xref:Whim.IWindow>s to <xref:Whim.IWorkspace>s to <xref:Whim.IMonitor>s.

The <xref:Whim.IButler.Pantry> property can be set up until initialization to customize the behavior of the mappings.

## `IInternalContext`

Similarly to `IContext`, there is an internal-only interface `Whim.IInternalContext` which contains internal core functionality which are not necessary to be exposed to plugins. This includes:

- the `ICoreSavedStateManager` to save core state
- the `ICoreNativeManager` wrapper for internal-only [CsWin32](./native-apis.md) APIs.
- hooks for Windows event providers, like `IKeybindHook`, `IMouseHook`, and `IWindowMessageMonitor`
- internal interface implementations of public managers - e.g., `MonitorManager` is an implementation of `IInternalMonitorManager` and `IMonitorManager`
- `IDeferWindowPosManager` to [handle STA re-entrancy](./threading.md)

Plugins should subscribe to Whim's various `public` events.

## Initialization

Managers will generally implement a subset of the following methods:

- `PreInitialize` - used for things like subscribing to event listeners
- `Initialize` - used for loading state, (usually user-defined or saved state)
- `PostInitialize` - used for actions which rely on other things being initialized, like creating windows, starting hooks, or subscribing to events which we only care about now that initialiation is complete

Items should make the objects which expose events have been initialized prior to the subscription.

> [!NOTE]
> The user should not initialize items, and should leave this to Whim.

## Butler

The "butler" or <xref:Whim.IButler> in Whim is responsible for using the <xref:Whim.IWorkspaceManager> and <xref:Whim.IMonitorManager> to handle events from the [window manager](#window-manager) to its "butler pantry". The butler also provides access to methods via inheritance from its "butler chores" to manage the movement of windows between workspaces and monitors.

The "butler pantry" or <xref:Whim.IButlerPantry> stores the assignment of windows to workspaces, and the assignment of workspaces to monitors.

When [scripting](../customize/scripting.md), use `IButler` methods to move windows between workspaces and monitors.

## Workspaces

Some <xref:IWorkspace> methods refer to `Workspace`-specific state:

- the currently active layout engine
- moving/focusing windows in a workspace

These methods will generally call <xref:Whim.IWorkspace.DoLayout()>.

Methods which are typically used for internal Whim functionality will not call `DoLayout()`. These methods will include comments in the documentation warning against using them in scripts or plugins. Misuse of these methods can lead to unexpected behavior, as workspaces become out of sync with the [butler pantry](#butler).

## Monitors

Whim supports multiple monitors via the <xref:Whim.IMonitorManager>, which stores the current monitor configuration. `IMonitorManager` provides various methods including the ability get adjacent monitors, and the monitor which contains a given point.

Each <xref:Whim.IMonitor> contains properties like its scale factor.

> [!NOTE]
> Whim does not support Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors.

## Window Manager

The "window manager" or <xref:Whim.IWindowManager> is used by Whim to manage <xref:Whim.IWindow>s. It listens to window events from Windows and notifies listeners (Whim core, plugins, etc.).

For example, the `WindowFocused` event is used by the `Whim.FocusIndicator` and `Whim.Bar` plugins to update their indications of the currently focused window.
