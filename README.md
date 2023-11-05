# Whim

Whim is a pluggable and modern window manager for Windows 10 and 11, built using WinUI 3 and .NET. It is currently in active development.

![Whim demo](docs/assets/readme-demo.gif)

## Installation

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases).

## Customization

When you run Whim for the first time, it will create a `.whim` directory in your user profile - for example, `C:\Users\Isaac\.whim`.

This directory will contain a `whim.config.csx` file which you can edit to customize Whim. This file is a C# script file, and is reloaded every time Whim starts. To have the best development experience, you should have dotnet tooling installed (Visual Studio Code will prompt you when you open `.whim`).

The config contains a pre-filled example which you can use as a starting point. You can also find the config [here](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx).

### Plugins

Whim is build around plugins. Plugins are referenced using `#r` and `using` statements at the top of the config file. Each plugin generally has a `Config` class, and a `Plugin` class. For example:

```csharp
BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
BarPlugin barPlugin = new(context, barConfig);
context.PluginManager.AddPlugin(barPlugin);
```

Each plugin needs to be added to the `context` object.

### Commands

Whim stores commands ([`ICommand`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Commands/ICommand.cs)), which are objects with a unique identifier, title, and executable action. Commands expose easy access to functionality from Whim's core, and loaded plugins.

Command identifiers namespaced to the plugin which defines them. For example, the `whim.core` namespace is reserved for core commands, and `whim.gaps` is used by the `GapsPlugin` to define commands. Identifiers are based on the [`Name`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Plugin/IPlugin.cs) property of the plugin - for example, [`GapsPlugin.Name`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim.Gaps/GapsPlugin.cs).

Each plugin can provide commands through the `PluginCommands` property of the [`IPlugin`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Plugin/IPlugin.cs) interface.

Custom commands can be created using the `whim.custom` namespace.

### Keybinds

Commands can be bound to keybinds ([`IKeybind`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Keybinds/IKeybind.cs)).

**Each command is bound to a single keybind.**

**Each keybind can trigger multiple commands.**

Keybinds can be overridden and removed in the config. For example:

```csharp
// Override the default keybind for showing/hiding the command palette.
context.KeybindManager.SetKeybind("whim.command_palette.toggle", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_P));

// Remove the default keybind for closing the current workspace.
context.KeybindManager.RemoveKeybind("whim.core.close_current_workspace);
```

## Inspiration

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project, to which I've contributed to in the past. However, there are a few key differences:

- Whim is built using WinUI 3 instead of Windows Forms. This makes it easier to have a more modern UI.
- Whim has a more powerful command palette, which supports fuzzy search.
- Whim stores windows internally in a more flexible way. This facilitates more complex window management. For more, see [Layouts](#layouts).
- Whim has a command system with common functionality, which makes it easier to interact with at a higher level.
- Creating subclasses of internal classes is not encouraged in Whim - instead, plugins should suffice to add new functionality.

Whim was not built to be a drop-in replacement for workspacer, but it does have a similar feel and many of the same features. It is not a fork of workspacer, and is built from the ground up.

It should be noted that [workspacer is no longer in active development](https://github.com/workspacer/workspacer/discussions/485).

I am grateful to the workspacer project for the inspiration and ideas it has provided.

## Architecture

> In progress...

### Layouts

This is one of the key areas where Whim differs from workspacer.

| Concept                                                             | workspacer             | Whim                                                  |
| ------------------------------------------------------------------- | ---------------------- | ----------------------------------------------------- |
| [Data structure for storing windows](#ilayoutengine-data-structure) | `IEnumerable<IWindow>` | Any                                                   |
| [Primary area support](#primary-area-support)                       | Yes                    | Not built in but possible in a custom `ILayoutEngine` |
| [Directional support](#directional-support)                         | No                     | Yes                                                   |
| [`ILayoutEngine` mutability](#ilayoutengine-mutability)             | Mutable                | Immutable                                             |

#### `ILayoutEngine` data structure

Currently, workspacer stores all windows in an [`IEnumerable<IWindow>`](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Workspace/Workspace.cs#L10) stack which is passed to each [`ILayout` implementation](https://github.com/workspacer/workspacer/blob/17750d1f84b8bb9015638ee7a733a2976ce08d25/src/workspacer.Shared/Layout/ILayoutEngine.cs#L23). Relying so heavily on a stack prevents workspacer from supporting more complex window layouts. For example, Whim's [`TreeLayoutEngine`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim.TreeLayout/TreeLayoutEngine.cs) uses a n-ary tree structure to store windows in arbitrary grid layouts.

#### Primary area support

Whim does not have a core concept of a "primary area", as it's an idea which lends itself to a stack-based data structure. However, it is possible to implement this functionality in a custom `ILayoutEngine` and plugin.

#### Directional support

As Whim supports more novel layouts, it also has functionality to account for directions, like `FocusWindowInDirection`, `SwapWindowInDirection`, and `MoveWindowEdgesInDirection`. For example, it's possible to drag a corner of a window diagonally to resize it (provided the underlying `ILayoutEngine` supports it).

#### `ILayoutEngine` mutability

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

### Visual Studio Code

The Whim repository includes a `.vscode` directory with a [`launch.json`](.vscode/launch.json) file. This file contains a `Launch Whim.Runner` configuration which can be used to debug Whim in Visual Studio Code. Unfortunately tests do not appear in Visual Studio Code's Test Explorer.

Tasks to build, test, and format XAML can be found in [`tasks.json`](.vscode/tasks.json).

To see the recommended extensions, open the Command Palette and run `Extensions: Show Recommended Extensions`.

## Discord

A Discord server has been set up at <https://discord.gg/gEFq9wr7jb>.
