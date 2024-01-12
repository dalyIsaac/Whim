# Whim

Whim is a pluggable and modern window manager for Windows 10 and 11, built using WinUI 3 and .NET. It is currently in active development.

![Whim demo](docs/images/demo.gif)

> [!NOTE]
> Documentation is lacking in some areas, and is a work in progress. If you have any questions, feel free to ask in the [Discord server](https://discord.gg/gEFq9wr7jb), or [raise an issue on GitHub](https://github.com/dalyIsaac/Whim/issues/new/choose).

#### Default Keybindings

Keybindings can also be seen in the command palette, when it is activated (<kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> by default).

##### Core Commands

These are the commands and associated keybindings provided by Whim's core. See [`CoreCommands.cs`](src/Whim/Commands/CoreCommands.cs).

| Identifier                                  | Title                                                            | Keybind                                              |
| ------------------------------------------- | ---------------------------------------------------------------- | ---------------------------------------------------- |
| `whim.core.activate_previous_workspace`     | Activate the previous workspace                                  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>LEFT</kbd>   |
| `whim.core.activate_next_workspace`         | Activate the next workspace                                      | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>RIGHT</kbd>  |
| `whim.core.focus_window_in_direction.left`  | Focus the window in the left direction                           | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>LEFT</kbd>    |
| `whim.core.focus_window_in_direction.right` | Focus the window in the right direction                          | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>RIGHT</kbd>   |
| `whim.core.focus_window_in_direction.up`    | Focus the window in the up direction                             | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>UP</kbd>      |
| `whim.core.focus_window_in_direction.down`  | Focus the window in the down direction                           | <kbd>Win</kbd> + <kbd>Alt</kbd> + <kbd>DOWN</kbd>    |
| `whim.core.swap_window_in_direction.left`   | Swap the window with the window to the left                      | <kbd>Win</kbd> + <kbd>LEFT</kbd>                     |
| `whim.core.swap_window_in_direction.right`  | Swap the window with the window to the right                     | <kbd>Win</kbd> + <kbd>RIGHT</kbd>                    |
| `whim.core.swap_window_in_direction.up`     | Swap the window with the window to the up                        | <kbd>Win</kbd> + <kbd>UP</kbd>                       |
| `whim.core.swap_window_in_direction.down`   | Swap the window with the window to the down                      | <kbd>Win</kbd> + <kbd>DOWN</kbd>                     |
| `whim.core.move_window_left_edge_left`      | Move the current window's left edge to the left                  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>H</kbd>      |
| `whim.core.move_window_left_edge_right`     | Move the current window's left edge to the right                 | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>J</kbd>      |
| `whim.core.move_window_right_edge_left`     | Move the current window's right edge to the left                 | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>K</kbd>      |
| `whim.core.move_window_right_edge_right`    | Move the current window's right edge to the right                | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>L</kbd>      |
| `whim.core.move_window_top_edge_up`         | Move the current window's top edge up                            | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>U</kbd>      |
| `whim.core.move_window_top_edge_down`       | Move the current window's top edge down                          | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>I</kbd>      |
| `whim.core.move_window_bottom_edge_up`      | Move the current window's bottom edge up                         | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>O</kbd>      |
| `whim.core.move_window_bottom_edge_down`    | Move the current window's bottom edge down                       | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>P</kbd>      |
| `whim.core.move_window_to_previous_monitor` | Move the window to the previous monitor                          | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.core.move_window_to_next_monitor`     | Move the window to the next monitor                              | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.core.focus_previous_monitor`          | Focus the previous monitor                                       | No default keybind                                   |
| `whim.core.focus_next_monitor`              | Focus the next monitor                                           | No default keybind                                   |
| `whim.core.close_current_workspace`         | Close the current workspace                                      | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>W</kbd>      |
| `whim.core.exit_whim`                       | Exit Whim                                                        | No default keybind                                   |
| `whim.core.activate_workspace_{idx}`        | Activate workspace `{idx}` (where `idx` is an int 1, 2, ...9, 0) | <kbd>Alt</kbd> + <kbd>Shift</kbd> + <kbd>{idx}</kbd> |

##### Command Palette Plugin Commands

See [`CommandPaletteCommands.cs`](src/Whim.CommandPalette/CommandPaletteCommands.cs).

| Identifier                                                | Title                              | Keybind                                          |
| --------------------------------------------------------- | ---------------------------------- | ------------------------------------------------ |
| `whim.command_palette.toggle`                             | Toggle command palette             | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> |
| `whim.command_palette.activate_workspace`                 | Activate workspace                 | No default keybind                               |
| `whim.command_palette.rename_workspace`                   | Rename workspace                   | No default keybind                               |
| `whim.command_palette.create_workspace`                   | Create workspace                   | No default keybind                               |
| `whim.command_palette.move_window_to_workspace`           | Move window to workspace           | No default keybind                               |
| `whim.command_palette.move_multiple_windows_to_workspace` | Move multiple windows to workspace | No default keybind                               |
| `whim.command_palette.remove_window`                      | Select window to remove from Whim  | No default keybind                               |

##### Floating Layout Plugin Commands

See [`FloatingLayoutCommands.cs`](src/Whim.FloatingLayout/FloatingLayoutCommands.cs).

| Identifier                                     | Title                   | Keybind                                          |
| ---------------------------------------------- | ----------------------- | ------------------------------------------------ |
| `whim.floating_layout.toggle_window_floating`  | Toggle window floating  | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>F</kbd> |
| `whim.floating_layout.mark_window_as_floating` | Mark window as floating | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>M</kbd> |
| `whim.floating_layout.mark_window_as_docked`   | Mark window as docked   | <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>D</kbd> |

##### Focus Indicator Plugin Commands

See [`FocusIndicatorCommands.cs`](src/Whim.FocusIndicator/FocusIndicatorCommands.cs).

| Identifier                            | Title                                         | Keybind            |
| ------------------------------------- | --------------------------------------------- | ------------------ |
| `whim.focus_indicator.show`           | Show focus indicator                          | No default keybind |
| `whim.focus_indicator.toggle`         | Toggle focus indicator                        | No default keybind |
| `whim.focus_indicator.toggle_fade`    | Toggle whether the focus indicator fades      | No default keybind |
| `whim.focus_indicator.toggle_enabled` | Toggle whether the focus indicator is enabled | No default keybind |

##### Gaps Plugin Commands

See [`GapsCommands.cs`](src/Whim.Gaps/GapsCommands.cs).

| Identifier                 | Title              | Keybind                                                            |
| -------------------------- | ------------------ | ------------------------------------------------------------------ |
| `whim.gaps.outer.increase` | Increase outer gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>L</kbd> |
| `whim.gaps.outer.decrease` | Decrease outer gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>H</kbd> |
| `whim.gaps.inner.increase` | Increase inner gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>K</kbd> |
| `whim.gaps.inner.decrease` | Decrease inner gap | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>J</kbd> |

##### Slice Layout Plugin Commands

See [`SliceLayoutCommands.cs`](src/Whim.SliceLayout/SliceLayoutCommands.cs).

| Identifier                                    | Title                              | Keybind            |
| --------------------------------------------- | ---------------------------------- | ------------------ |
| `whim.slice_layout.set_insertion_type.swap`   | Set slice insertion type to swap   | No default keybind |
| `whim.slice_layout.set_insertion_type.rotate` | Set slice insertion type to rotate | No default keybind |
| `whim.slice_layout.window.promote`            | Promote window in stack            | No default keybind |
| `whim.slice_layout.window.demote`             | Demote window in stack             | No default keybind |
| `whim.slice_layout.focus.promote`             | Promote focus in stack             | No default keybind |
| `whim.slice_layout.focus.demote`              | Demote focus in stack              | No default keybind |

##### Tree Layout Plugin Commands

See [`TreeLayoutCommands.cs`](src/Whim.TreeLayout/TreeLayoutCommands.cs).

| Identifier                                  | Title                                          | Keybind                                                                |
| ------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------- |
| `whim.tree_layout.add_tree_direction_left`  | Add windows to the left of the current window  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.tree_layout.add_tree_direction_right` | Add windows to the right of the current window | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.tree_layout.add_tree_direction_up`    | Add windows above the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>UP</kbd>    |
| `whim.tree_layout.add_tree_direction_down`  | Add windows below the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>DOWN</kbd>  |

### Window Manager

The [`IWindowManager`](src/Whim/Window/IWindowManager.cs) is used by Whim to manage [`IWindow`](src/Whim/Window/IWindow.cs)s. It listens to window events from Windows and notifies listeners (Whim core, plugins, etc.).

For example, the `WindowFocused` event is used by the `Whim.FocusIndicator` and `Whim.Bar` plugins to update their indications of the currently focused window.

The `IWindowManager` also exposes an `IFilterManager` called `LocationRestoringFilterManager`. Some applications like to restore their window positions when they start (e.g., Firefox, JetBrains Gateway). As a window manager, this is undesirable. `LocationRestoringFilterManager` listens to `WindowMoved` events for these windows and will force their parent `IWorkspace` to do a layout two seconds after their first `WindowMoved` event, attempting to restore the window to its correct position.

If this doesn't work, dragging a window's edge will force a layout, which should fix the window's position. This is an area which could use further improvement.

### Logging

Whim wraps [Serilog](https://serilog.net/) to provide logging functionality. It can be configured using the [`LoggerConfig`](src/Whim/Logging/LoggerConfig.cs) class. For example:

```csharp
// The logger will only log messages with a level of `Debug` or higher.
context.Logger.Config = new LoggerConfig() { BaseMinLogLevel = LogLevel.Debug };

// The logger will log messages with a level of `Debug` or higher to a file.
if (context.Logger.Config.FileSink is FileSinkConfig fileSinkConfig)
{
    fileSinkConfig.MinLogLevel = LogLevel.Debug;
}

// The logger will log messages with a level of `Error` or higher to the debug console.
// The debug sink is only available in debug builds, and can slow down Whim.
if (context.Logger.Config.DebugSink is SinkConfig debugSinkConfig)
{
    debugSinkConfig.MinLogLevel = LogLevel.Error;
}
```

Logging can be changed during runtime to be more restrictive, but cannot be made more permissive than the initial configuration.

## Automatic Updating

The `Whim.Updater` plugin is in `alpha` (especially as Whim hasn't started releasing non-`alpha` builds). If the updater fails, you can manually update Whim by downloading the latest release from the [releases page](https://github.com/dalyIsaac/Whim/releases).

The updater will show a notification when a new version is available. Clicking on the notification will show the changelog for the delta between the current version and the latest version.

The `UpdaterConfig` supports specifying the `ReleaseChannel` and `UpdateFrequency`.

## Architecture

### Inspiration

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project, to which I've contributed to in the past. However, there are a few key differences:

- Whim is built using WinUI 3 instead of Windows Forms. This makes it easier to have a more modern UI.
- Whim has a more powerful command palette, which supports fuzzy search.
- Whim stores windows internally in a more flexible way. This facilitates more complex window management. For more, see [Layouts](#layouts).
- Whim has a command system with common functionality, which makes it easier to interact with at a higher level.
- Creating subclasses of internal classes is not encouraged in Whim - instead, plugins should suffice to add new functionality.

Whim was not built to be a drop-in replacement for workspacer, but it does have a similar feel and many of the same features. It is not a fork of workspacer, and is built from the ground up.

It should be noted that [workspacer is no longer in active development](https://github.com/workspacer/workspacer/discussions/485).

I am grateful to the workspacer project for the inspiration and ideas it has provided.

### Layouts

This is one of the key areas where Whim differs from workspacer.

| Concept                                                             | workspacer             | Whim                                                  |
| ------------------------------------------------------------------- | ---------------------- | ----------------------------------------------------- |
| [Data structure for storing windows](#ilayoutengine-data-structure) | `IEnumerable<IWindow>` | Any                                                   |
| [Primary area support](#primary-area-support)                       | Yes                    | Not built in but possible in a custom `ILayoutEngine` |
| [Directional support](#directional-support)                         | No                     | Yes                                                   |
| [`ILayoutEngine` mutability](#ilayoutengine-mutability)             | Mutable                | Immutable                                             |

#### `ILayoutEngine` Data Structure

Currently, workspacer stores all windows in an [`IEnumerable<IWindow>`](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Workspace/Workspace.cs#L10) stack which is passed to each [`ILayout` implementation](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Layout/ILayoutEngine.cs#L23). Relying so heavily on a stack prevents workspacer from supporting more complex window layouts. For example, Whim's [`TreeLayoutEngine`](src/Whim.TreeLayout/TreeLayoutEngine.cs) uses a n-ary tree structure to store windows in arbitrary grid layouts.

#### Primary Area Support

Whim does not have a core concept of a "primary area", as it's an idea which lends itself to a stack-based data structure. However, it is possible to implement this functionality in a custom `ILayoutEngine` and plugin.

#### Directional Support

As Whim supports more novel layouts, it also has functionality to account for directions, like `FocusWindowInDirection`, `SwapWindowInDirection`, and `MoveWindowEdgesInDirection`. For example, it's possible to drag a corner of a window diagonally to resize it (provided the underlying `ILayoutEngine` supports it).

#### `ILayoutEngine` Mutability

Implementations of Whim's `ILayoutEngine` should be immutable. This was done to support functionality like previewing changes to layouts before committing them, with the `LayoutPreview` plugin. In comparison, workspacer's `ILayoutEngine` implementations are mutable.

## Contributing

Please file an issue if you find any bugs or have any feature requests, or ask in the Discord. Pull requests are welcome.

Work is currently being tracked in the [project board](https://github.com/users/dalyIsaac/projects/2/views/7).

### Development Environment Setup

1. Clone this repo
2. Install tools for the Windows App SDK:
   1. Go to the [Install tools for the Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment) page
   2. Follow the instructions for [winget for C# developers](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b#for-c-developers), or [installing Visual Studio](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b#install-visual-studio)

After cloning, make sure to run the following in a shell in the root Whim directory:

```shell
git config core.autocrlf true
```

If you've already made changes with `core.autocrlf` set to `false`, you can fix the line endings with:

```shell
git add . --renormalize
```

Before making a pull request, please install the tools specified in [`.config/dotnet-tools.json`](.config/dotnet-tools.json):

```shell
dotnet tool restore
# To run the formatters:
dotnet tool run dotnet-csharpier .
dotnet tool run xstyler --recursive --d . --config ./.xamlstylerrc
```

### Building

#### Visual Studio

Visual Studio 2022 is the easiest way to get started with working on Whim. Check the following:

- `nuget.org` is added to the [Package Sources](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#package-sources)
- The **Configuration Manager** is set to `Debug` and your target architecture is correct (e.g. `x64`).
- Each project's platform matches the current target architecture.
- `Whim.Runner` is set as the startup project.
- The **green Start arrow** is labeled `Whim.Runner (Unpackaged)`.

**Recommended Extensions:**

- [CSharpier](https://marketplace.visualstudio.com/items?itemName=csharpier.CSharpier)
- [XAML Styler for Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler2022)

> [!WARNING]
>
> Windows App SDK 1.4 introduced a bug which causes Visual Studio to crash Whim when debugging. Make sure to apply the workaround from <https://github.com/microsoft/microsoft-ui-xaml/issues/9008#issuecomment-1773734685/>.

#### Visual Studio Code

The Whim repository includes a `.vscode` directory with a [`launch.json`](.vscode/launch.json) file. This file contains a `Launch Whim.Runner` configuration which can be used to debug Whim in Visual Studio Code. Unfortunately tests do not appear in Visual Studio Code's Test Explorer.

Check that `nuget.org` is added to the [Package Sources](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#package-sources).

Tasks to build, test, and format XAML can be found in [`tasks.json`](.vscode/tasks.json).

To see the recommended extensions, open the Command Palette and run `Extensions: Show Recommended Extensions`.

### Unhandled Exception Handling

`IContext` has an `UncaughtExceptionHandling` property to specify how to handle uncaught exceptions. When developing, it's recommended to set this to `UncaughtExceptionHandling.Shutdown` to shutdown Whim when an uncaught exception occurs. This will make it easier to debug the exception.

All uncaught exceptions will be logged as `Fatal`.

### Tests

Tests have not been written for all of Whim's code, but are encouraged. Tests have not been written for UI code-behind files, as I committed to xUnit before I realized that Windows App SDK isn't easily compatible with xUnit. I'm open to suggestions on how to test UI code-behind files.

### Updating `#r` directives

To use your existing configuration, make sure to update the `#r` directives to point to your newly compiled DLLs. In other words, replace `C:\Users\<USERNAME>\AppData\Local\Programs\Whim` with `C:\path\to\repo\Whim`:

```csharp
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\whim.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Bar\Whim.Bar.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"

// Old references:
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\whim.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.Bar\Whim.Bar.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.Gaps\Whim.Gaps.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
```

Alternatively, the `#r` directives can be specified using a magic path prefix `WHIM_PATH` that is automatically replaced by the assembly's path when reading the config file:

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
```
