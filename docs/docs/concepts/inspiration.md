# Inspiration

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project. Whim was not built to be a drop-in replacement for workspacer, but it does have a similar feel and many of the same features. It is not a fork of workspacer, and is built from the ground up.

It should be noted that [workspacer is no longer in active development](https://github.com/workspacer/workspacer/discussions/485). I am grateful to the workspacer project for the inspiration and ideas it has provided.

There are several key differences between Whim and workspacer:

## Layout Engines

workspacer stores all windows in an [`IEnumerable<IWindow>`](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Workspace/Workspace.cs#L10) stack which is passed to each [`ILayout` implementation](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Layout/ILayoutEngine.cs#L23). This has a number of downsides:

- All layout engines must work with the same list of windows.
- workspacer cannot support more complex window layouts. In comparison, each <xref:Whim.ILayoutEngine> in Whim stores windows in their own manner. For example, Whim's [tree layout](../plugins/tree-layout.md) uses a n-ary tree structure to store windows in arbitrary grid layouts. For more, see [Layout Engines](layout-engines.md).

Whim's <xref:Whim.ILayoutEngine>s also have methods for directional operations, like:

- <xref:Whim.ILayoutEngine.FocusWindowInDirection(Whim.Direction,Whim.IWindow)>
- <xref:Whim.ILayoutEngine.SwapWindowInDirection(Whim.Direction,Whim.IWindow)>
- <xref:Whim.IWorkspace.MoveWindowEdgesInDirection(Whim.Direction,Whim.IPoint{System.Double},Whim.IWindow,System.Boolean)>

Whim's <xref:Whim.ILayoutEngine> does not have the concept of a "primary area". However, this can be provided by `ILayoutEngine` implementations - for example, the <xref:Whim.SliceLayout.SliceLayoutEngine>.

Implementations of Whim's `ILayoutEngine` should be immutable. This was done to support functionality like previewing changes to layouts before committing them, with the [Layout Preview](../plugins/layout-preview.md) plugin. In comparison, workspacer's `ILayoutEngine` implementations are mutable.

## Commands

Whim has a command system with common functionality, which makes it easier to interact with at a higher level. The command system is used by different components, like the <xref:Whim.IKeybindManager> and [Command Palette](../plugins/command-palette.md).

The command palette in Whim is also more powerful than the one in , using a port of the Visual Studio Code command palette fuzzy search algorithm.

## Appearance

Whim is built on top of WinUI 3 instead of Windows Forms. This makes it easier to have a more modern-looking UI, and means it's possible for users to specify styling using XAML - see [Styling](../styling.md).
