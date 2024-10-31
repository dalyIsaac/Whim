# Introduction

<!-- NOTE: This is largely duplicated to the README.md -->

Whim is a hackable, pluggable and scriptable dynamic window manager for Windows.

Whim lets you easily layout your windows in a way that suits your workflow via [keyboard shortcuts](configure/core/keybinds.md), dragging windows using your mouse, or plugins including a [command palette](configure/plugins/command-palette.md) or [bar](configure/plugins/bar.md).

## Key Features ✨

- **Multiple Layout Engines**: Choose from various [layout engines](configure/core/layout-engines.md) to suit your workflow.
- **Simple Configuration**: Use [YAML or JSON](configure/yaml-json-configuration.md) for easy setup.
- **Hackable and Pluggable**: Extend Whim with powerful [C# scripts](script/scripting.md).
- **Workspace Management**: Manage windows efficiently across multiple [workspaces](configure/core/workspaces.md).
- **User-Centric Design**: Full control over your window management.

> All the power to the user!

![Whim demo](images/demo.gif)

## Installation 🛠️

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases) on GitHub. Whim has an [updater plugin](configure/plugins/updater.md) to notify you of new releases.

Installation via package managers is coming in [dalyIsaac/Whim#792](https://github.com/dalyIsaac/Whim/issues/792).

## Why use Whim? 🤔

A window manager is responsible for controlling the layout of windows in your desktop environment. Whim is a [dynamic window manager](https://en.wikipedia.org/wiki/Dynamic_window_manager), where windows are arranged according to different layouts.

Whim supports multiple layout engines. Each [workspace](configure/core/workspaces.md) can switch between different layout engines. For example, the [`TreeLayoutEngine`](configure/core/layout-engines.md#tree) allows users to create arbitrary grids of windows during runtime (similar to `i3`), while the [`SliceLayoutEngine`](configure/core/layout-engines.md#slice) fully automates windows placement using a predefined, customizable logic (similar to `Awesome` or `dwm`) . For more, see [Layout Engines](configure/core/layout-engines.md).

Whim has a simple configuration system that uses [YAML or JSON](configure/yaml-json-configuration.md). Whim also has a [C# scripting system](script/scripting.md) for more advanced users. This means you can start out with a simple configuration and gradually add more complex functionality, using the full power of C#. Whim exposes its API for plugins to use. Accordingly, much of the more custom functionality has been implemented as plugins which users can choose to use or not.

Whim works by sitting on top of Windows' own window manager. It listens to window events and moves windows accordingly. Whim does not use Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors. Instead, Whim has [workspaces](configure/core/workspaces.md).

To see how Whim compares to other Windows window managers, see [Whim vs. Other Window Managers](intro/comparison.md).

## Contributing 🤝

Whim is an open-source project and contributions are welcome! For more information, see the [Contributing](contribute/guide.md) page.

## Community 🌐

Whim has a [Discord server](https://discord.gg/gEFq9wr7jb) where you can ask questions, get help, and chat with other users.

## Thanks 🙏

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project - for more, see the [Inspiration](intro/inspiration.md) page.

Thank you to all the contributors to Whim for their help and support! 💖
