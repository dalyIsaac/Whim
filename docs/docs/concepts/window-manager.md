# Window Manager

The "window manager" or <xref:Whim.IWindowManager> is used by Whim to manage <xref:Whim.IWindow>s. It listens to window events from Windows and notifies listeners (Whim core, plugins, etc.).

For example, the `WindowFocused` event is used by the `Whim.FocusIndicator` and `Whim.Bar` plugins to update their indications of the currently focused window.

The `IWindowManager` also exposes an `IFilterManager` called `LocationRestoringFilterManager`. Some applications like to restore their window positions when they start (e.g., Firefox, JetBrains Gateway). As a window manager, this is undesirable. `LocationRestoringFilterManager` listens to `WindowMoved` events for these windows and will force their parent `IWorkspace` to do a layout two seconds after their first `WindowMoved` event, attempting to restore the window to its correct position.

If this doesn't work, dragging a window's edge will force a layout, which should fix the window's position. This is an area which could use further improvement.
