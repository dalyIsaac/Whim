# Architecture

## Inspiration

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project. Whim was not built to be a drop-in replacement for workspacer, but it does have a similar feel and many of the same features. It is not a fork of workspacer, and is built from the ground up.

It should be noted that [workspacer is no longer in active development](https://github.com/workspacer/workspacer/discussions/485). I am grateful to the workspacer project for the inspiration and ideas it has provided.

There are several key differences between Whim and workspacer:

### Layout Engines

workspacer stores all windows in an [`IEnumerable<IWindow>`](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Workspace/Workspace.cs#L10) stack which is passed to each [`ILayout` implementation](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Layout/ILayoutEngine.cs#L23). This has a number of downsides:

- All layout engines must work with the same list of windows.
- workspacer cannot support more complex window layouts. In comparison, each <xref:Whim.ILayoutEngine> in Whim stores windows in their own manner. For example, Whim's [tree layout](plugins/tree-layout.md) uses a n-ary tree structure to store windows in arbitrary grid layouts. For more, see [Layout Engines](layout-engines.md).

Whim's <xref:Whim.ILayoutEngine>s also have methods for directional operations, like <xref:Whim.ILayoutEngine.FocusWindowInDirection(Whim.Direction,Whim.IWindow)>, <xref:Whim.ILayoutEngine.SwapWindowInDirection(Whim.Direction,Whim.IWindow)> and <xref:Whim.IWorkspace.MoveWindowEdgesInDirection(Whim.Direction,Whim.IPoint{System.Double},Whim.IWindow)>.

Whim's <xref:Whim.ILayoutEngine> does not have the concept of a "primary area". However, this can be provided by `ILayoutEngine` implementations - for example, the <xref:Whim.SliceLayout.SliceLayoutEngine>.

Implementations of Whim's `ILayoutEngine` should be immutable. This was done to support functionality like previewing changes to layouts before committing them, with the `LayoutPreview` plugin. In comparison, workspacer's `ILayoutEngine` implementations are mutable.

### Commands

Whim has a command system with common functionality, which makes it easier to interact with at a higher level. The command system is used by different components, like the <xref:Whim.IKeybindManager> and [Command Palette](plugins/command-palette.md).

The command palette in Whim is also more powerful than the one in , using a port of the Visual Studio Code command palette fuzzy search algorithm.

### Appearance

Whim is built on top of WinUI 3 instead of Windows Forms. This makes it easier to have a more modern-looking UI, and means it's possible for users to specify styling using XAML - see [Styling](styling.md).

## `IContext`

The <xref:Whim.IContext> consists of:

- the <xref:Whim.IButler>
- managers which contain and control Whim's state and functionality
- events related to the `IContext`
- the <xref:Whim.IContext.UncaughtExceptionHandling> setting
- the <xref:Whim.Logger>.

The <xref:Whim.IButler> uses the various managers to handle events from Windows and the user to update the mapping of <xref:Whim.IWindow>s to <xref:Whim.IWorkspace>s to <xref:Whim.IMonitor>s.

The <xref:Whim.IButler.Pantry> property can be set up until initialization to customize the behavior of the mappings.

### `IInternalContext`

Similarly to `IContext`, there is an internal-only interface `Whim.IInternalContext` which contains internal core functionality which are not necessary to be exposed to plugins. This includes:

- the `ICoreSavedStateManager` to save core state
- hooks for Windows event providers, like `IKeybindHook`, `IMouseHook`, and `IWindowMessageMonitor`
- internal interface implementations of public managers - e.g., `MonitorManager` is an implementation of `IInternalMonitorManager` and `IMonitorManager`
- `IDeferWindowPosManager` to [handle STA re-entrancy](#single-threaded-apartments)

Plugins should subscribe to Whim's various `public` events.

### Initialization

Managers will generally implement a subset of the following methods:

- `PreInitialize` - used for things like subscribing to event listeners
- `Initialize` - used for loading state, (usually user-defined or saved state)
- `PostInitialize` - used for actions which rely on other things being initialized, like creating windows, starting hooks, or subscribing to events which we only care about now that initialiation is complete

Items should make the objects which expose events have been initialized prior to the subscription.

### CsWin32

Whim uses the [CsWin32](https://github.com/microsoft/CsWin32) source generator to add Win32 P/Invoke methods and types. The list of items added can be found in `NativeMethods.txt`.

## Events

There are a variety of ways that Whim receives events:

- `IWindowMessageMonitor`, to receive `WM_MESSAGE` events in a dedicated Whim window whose only purpose is to receive said events
- `IKeybindHook` to receive all keyboard events from `WH_KEYBOARD_LL`
- `IMouseHook`, to receive all mouse events from `WH_MOUSE_LL`
- `INotificationManager`, to receive events from user interactions with Windows events
- WinUI elements like windows, to receive user interactions with visual elements

Each of these entrypoints are wrapped with <xref:Whim.IContext.HandleUncaughtException>. The behavior of this wrapper method can be customized using <xref:Whim.IContext.HandleUncaughtException(System.String,System.Exception)>.

## Single Threaded Apartments

Whim operates on a single thread and uses the [Single-Threaded Apartments (STAs)](https://learn.microsoft.com/en-us/windows/win32/com/single-threaded-apartments) model. Unfortunately, Whim is subject to reentrancy as part of using STAs. For a good overview of reentrancy, see [What is so special about the Application STA?](https://devblogs.microsoft.com/oldnewthing/20210224-00/?p=104901) by Raymond Chen's The New Old Thing blog.

STA reentrancy has caused issues where Whim would:

1. Enter and do some processing
2. While waiting for something, enter again due to another event, and set the window positions
3. Go back to the deferred block of execution and set the window positions

This could cause loops where a sort of infinite loop would be entered, as windows would continually move between different positions ([dalyIsaac/Whim#446](https://github.com/dalyIsaac/Whim/issues/446)).

As it is "[not possible to prevent reentrancy](https://learn.microsoft.com/en-us/windows/win32/winauto/guarding-against-reentrancy-in-hook-functions)", Whim has a `DeferWindowPosManager` which checks for reentrancy in the current stack. If reentrancy has been detected, `DeferWindowPosHandle` has its layout deferred until there are no reentrant execution blocks in the stack. This was implemented in [dalyIsaac/Whim#553](https://github.com/dalyIsaac/Whim/pull/553).

On a similar vein, `Workspace` has a method `GarbageCollect` which attempts to remove windows from the workspace which are no longer valid.
