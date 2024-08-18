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

Managers and the store will generally implement a subset of the following methods:

- `PreInitialize` - used for things like subscribing to event listeners
- `Initialize` - used for loading state, (usually user-defined or saved state)
- `PostInitialize` - used for actions which rely on other things being initialized, like creating windows, starting hooks, or subscribing to events which we only care about now that initialiation is complete

Items should make the objects which expose events have been initialized prior to the subscription.

> [!NOTE]
> The user should not initialize items, and should leave this to Whim.
