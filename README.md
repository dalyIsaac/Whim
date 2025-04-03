# [Whim](https://dalyisaac.github.io/Whim) 🪟

<!-- NOTE: This is largely a duplicate of docs/docs/index.md -->

Whim is a hackable, pluggable and scriptable dynamic window manager for Windows.

Whim lets you easily layout your windows in a way that suits your workflow via [keyboard shortcuts](https://dalyisaac.github.io/Whim/configure/core/keybinds.html), dragging windows using your mouse, or plugins including a [command palette](https://dalyisaac.github.io/Whim/configure/plugins/command-palette.html) or [bar](https://dalyisaac.github.io/Whim/configure/plugins/bar.html).

## Key Features ✨

- **Multiple Layout Engines**: Choose from various [layout engines](https://dalyisaac.github.io/Whim/configure/core/layout-engines.html) to suit your workflow.
- **Simple Configuration**: Use [YAML or JSON](https://dalyisaac.github.io/Whim/configure/yaml-json-configuration.html) for easy setup.
- **Hackable and Pluggable**: Extend Whim with powerful [C# scripts](https://dalyisaac.github.io/Whim/script/scripting.html).
- **Workspace Management**: Manage windows efficiently across multiple [workspaces](https://dalyisaac.github.io/Whim/configure/core/workspaces.html).
- **User-Centric Design**: Full control over your window management.

> All the power to the user!

![Whim demo](docs/images/readme.gif)

## Installation 🛠️

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases) on GitHub. Whim has an [updater plugin](https://dalyisaac.github.io/Whim/docs/plugins/updater.html) to keep you up to date.

Installation via package managers is coming in [dalyIsaac/Whim#792](https://github.com/dalyIsaac/Whim/issues/792).

## Why use Whim? 🤔

A window manager is responsible for controlling the layout of windows in your desktop environment. Whim is a [dynamic window manager](https://en.wikipedia.org/wiki/Dynamic_window_manager), where windows are arranged according to different layouts.

Whim supports multiple layout engines. Each [workspace](https://dalyisaac.github.io/Whim/docs/customize/workspaces.html) can switch between different layout engines. For example, the `TreeLayoutEngine` allows users to create arbitrary grids of windows during runtime (similar to `i3`), while the `SliceLayoutEngine` fully automates windows placement using a predefined, customizable logic (similar to `Awesome` or `dwm`) . For more, see [Layout Engines](https://dalyisaac.github.io/Whim/docs/customize/layout-engines.html).

Whim has a simple configuration system that uses [YAML or JSON](https://dalyisaac.github.io/Whim/configure/yaml-json-configuration.html). Whim also has a [C# scripting system](https://dalyisaac.github.io/Whim/script/scripting.html) for more advanced users. This means you can start out with a simple configuration and gradually add more complex functionality, using the full power of C#. Whim exposes its API for plugins to use. Accordingly, much of the more custom functionality has been implemented as plugins which users can choose to use or not.

Whim works by sitting on top of Windows' own window manager. It listens to window events and moves windows accordingly. Whim does not use Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors. Instead, Whim has [workspaces](https://dalyisaac.github.io/Whim/docs/customize/workspaces.html).

To see how Whim compares to other Windows window managers, see [Whim vs. Other Window Managers](https://dalyisaac.github.io/Whim/intro/comparison.html).

## Documentation 📖

You can find the Whim documentation at [dalyisaac.github.io/Whim](https://dalyisaac.github.io/Whim).

## Contributing 🤝

Whim is an open-source project and contributions are welcome! For more information, see the [Contributing](https://dalyisaac.github.io/Whim/docs/contribute/guide.html) page.

## Community 🌐

Whim has a [Discord server](https://discord.gg/gEFq9wr7jb) where you can ask questions, get help, and chat with other users.

## Thanks 🙏

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project - for more, see the [Inspiration](https://dalyisaac.github.io/Whim/getting-started/inspiration.html) page.

Thank you to all the contributors to Whim for their help and support! 💖
