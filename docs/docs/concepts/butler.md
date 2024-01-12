# Butler

The "butler" or <xref:Whim.IButler> in Whim is responsible for using the <xref:Whim.IWorkspaceManager> and <xref:Whim.IMonitorManager> to handle events from the [window manager](window-manager.md) to its "butler pantry". The butler also provides access to methods via inheritance from its "butler chores" to manage the movement of windows between workspaces and monitors.

The "butler pantry" or <xref:Whim.IButlerPantry> stores the assignment of windows to workspaces, and the assignment of workspaces to monitors.

When [scripting](../scripting.md), use `IButler` methods to move windows between workspaces and monitors.
