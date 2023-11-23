# Whim

Whim is a pluggable and modern window manager for Windows 10 and 11, built using WinUI 3 and .NET. It is currently in active development.

![Whim demo](docs/assets/readme-demo.gif)

> [!NOTE]
> Documentation is lacking in some areas, and is a work in progress. If you have any questions, feel free to ask in the [Discord server](https://discord.gg/gEFq9wr7jb), or [raise an issue on GitHub](https://github.com/dalyIsaac/Whim/issues/new/choose).

## Installation

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases).

## Customization

When you run Whim for the first time, it will create a `.whim` directory in your user profile - for example, `C:\Users\Isaac\.whim`.

This directory will contain a `whim.config.csx` file which you can edit to customize Whim. This file is a C# script file, and is reloaded every time Whim starts. To have the best development experience, you should have dotnet tooling installed (Visual Studio Code will prompt you when you open `.whim`).

The config contains a pre-filled example which you can use as a starting point. You can also find the config [here](src/Whim/Template/whim.config.csx).

### Plugins

Whim is build around plugins. Plugins are referenced using `#r` and `using` statements at the top of the config file. Each plugin generally has a `Config` class, and a `Plugin` class. For example:

```csharp
BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
BarPlugin barPlugin = new(context, barConfig);
context.PluginManager.AddPlugin(barPlugin);
```

Each plugin needs to be added to the `context` object.

### Commands

Whim stores commands ([`ICommand`](src/Whim/Commands/ICommand.cs)), which are objects with a unique identifier, title, and executable action. Commands expose easy access to functionality from Whim's core, and loaded plugins.

Command identifiers namespaced to the plugin which defines them. For example, the `whim.core` namespace is reserved for core commands, and `whim.gaps` is used by the `GapsPlugin` to define commands. Identifiers are based on the [`Name`](src/Whim/Plugin/IPlugin.cs) property of the plugin - for example, [`GapsPlugin.Name`](src/Whim.Gaps/GapsPlugin.cs).

Each plugin can provide commands through the `PluginCommands` property of the [`IPlugin`](src/Whim/Plugin/IPlugin.cs) interface.

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
                context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow?.Focus();
            }
        }
    );

    // Create an associated keybind.
    context.KeybindManager.SetKeybind("whim.custom.minimize_discord", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_D));

    // ...
}
```

### Keybinds

Commands can be bound to keybinds ([`IKeybind`](src/Whim/Keybinds/IKeybind.cs)).

**Each command is bound to a single keybind.**

**Each keybind can trigger multiple commands.**

Keybinds can be overridden and removed in the config. For example:

```csharp
// Override the default keybind for showing/hiding the command palette.
context.KeybindManager.SetKeybind("whim.command_palette.toggle", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_P));

// Remove the default keybind for closing the current workspace.
context.KeybindManager.RemoveKeybind("whim.core.close_current_workspace");
```

> [!WARNING]
> When overridding keybinds for plugins, make sure to set the keybind **after** calling `context.PluginManager.AddPlugin(plugin)`.
>
> Otherwise, `PluginManager.AddPlugin` will set the default keybinds, overriding custom keybinds set before the plugin is added.

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

##### Tree Layout Plugin Commands

See [`TreeLayoutCommands.cs`](src/Whim.TreeLayout/TreeLayoutCommands.cs).

| Identifier                                  | Title                                          | Keybind                                                                |
| ------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------- |
| `whim.tree_layout.add_tree_direction_left`  | Add windows to the left of the current window  | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>LEFT</kbd>  |
| `whim.tree_layout.add_tree_direction_right` | Add windows to the right of the current window | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>RIGHT</kbd> |
| `whim.tree_layout.add_tree_direction_up`    | Add windows above the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>UP</kbd>    |
| `whim.tree_layout.add_tree_direction_down`  | Add windows below the current window           | <kbd>Win</kbd> + <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>DOWN</kbd>  |

### Routing

[`IRouterManager`](src/Whim/Router/IRouterManager.cs) is used by Whim to route windows to specific workspaces. For example, to route Discord to a workspace "Chat", you can do the following:

```csharp
context.RouterManager.Add((window) =>
{
    if (window.ProcessFileName == "Discord.exe")
    {
        return context.WorkspaceManager.TryGet("Chat");
    }

    // Continue routing.
    return null;
});
```

### Filtering

[`IFilterManager`](src/Whim/Router/IFilterManager.cs) tells Whim to ignore windows based on `Filter` delegates. A common use case is for plugins to filter out windows they manage themselves and want Whim to not lay out. For example, the bars and command palette are filtered out.

```csharp
// Called by the bar plugin.
context.FilterManager.AddTitleMatchFilter("Whim Bar");
```

## Window Manager

The [`IWindowManager`](src/Whim/Window/IWindowManager.cs) is used by Whim to manage [`IWindow`](src/Whim/Window/IWindow.cs)s. It listens to window events from Windows and notifies listeners (Whim core, plugins, etc.).

For example, the `WindowFocused` event is used by the `Whim.FocusIndicator` and `Whim.Bar` plugins to update their indications of the currently focused window.

The `IWindowManager` also exposes an `IFilterManager` called `LocationRestoringFilterManager`. Some applications like to restore their window positions when they start (e.g., Firefox, JetBrains Gateway). As a window manager, this is undesirable. `LocationRestoringFilterManager` listens to `WindowMoved` events for these windows and will force their parent `IWorkspace` to do a layout two seconds after their first `WindowMoved` event, attempting to restore the window to its correct position.

If this doesn't work, dragging a window's edge will force a layout, which should fix the window's position. This is an area which could use further improvement.

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

Implementations of Whim's `ILayoutEngine` should be immutable. This was done to support future functionality like previewing changes to layouts before committing them (see [#425](https://github.com/dalyIsaac/Whim/issues/425)). In comparison, workspacer's `ILayoutEngine` implementations are mutable.

## Contributing

Please file an issue if you find any bugs or have any feature requests. Pull requests are welcome.

Work is currently being tracked in the [project board](https://github.com/users/dalyIsaac/projects/2/views/7).

Before making a pull request, please install the tools specified in [`.config/dotnet-tools.json`](.config/dotnet-tools.json):

```shell
dotnet tool restore
# To run the formatters:
dotnet tool run dotnet-csharpier .
dotnet tool run xstyler --recursive --d . --config ./.xamlstylerrc
```

Tests have not been written for all of Whim's code, but they are encouraged. Tests have not been written for UI code-behind files, as I committed to xUnit before I realized that Windows App SDK isn't easily compatible with xUnit. I'm open to suggestions on how to test UI code-behind files.

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

### Visual Studio

Visual Studio 2022 is the easiest way to get started with working on Whim. Check the following:

- The `.NET Desktop Development` workload is installed (see the Visual Studio Installer).
- The **Configuration Manager** is set to `Debug` and your target architecture (e.g. `x64`).
- Each project's platform matches the current target architecture.
- `Whim.Runner` is set as the startup project.
- The **green Start arrow** is labeled `Whim.Runner (Unpackaged)`.

**Recommended Extensions:**

- [CSharpier](https://marketplace.visualstudio.com/items?itemName=csharpier.CSharpier)
- [XAML Styler for Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler2022)

> [!WARNING]
>
> Windows App SDK 1.4 introduced a bug which causes Visual Studio to crash Whim when debugging. Make sure to apply the workaround from <https://github.com/microsoft/microsoft-ui-xaml/issues/9008#issuecomment-1773734685/>.

### Visual Studio Code

The Whim repository includes a `.vscode` directory with a [`launch.json`](.vscode/launch.json) file. This file contains a `Launch Whim.Runner` configuration which can be used to debug Whim in Visual Studio Code. Unfortunately tests do not appear in Visual Studio Code's Test Explorer.

Tasks to build, test, and format XAML can be found in [`tasks.json`](.vscode/tasks.json).

To see the recommended extensions, open the Command Palette and run `Extensions: Show Recommended Extensions`.

### Unhandled Exception Handling

`IContext` has an `UncaughtExceptionHandling` property to specify how to handle uncaught exceptions. When developing, it's recommended to set this to `UncaughtExceptionHandling.Shutdown` to shutdown Whim when an uncaught exception occurs. This will make it easier to debug the exception.

All uncaught exceptions will be logged as `Fatal`.
