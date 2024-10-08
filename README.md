# [Whim](https://dalyisaac.github.io/Whim)

<!-- NOTE: This is largely a duplicate of docs/docs/index.md -->

Whim is a hackable, pluggable and scriptable dynamic window manager for Windows 10 and 11, built using WinUI 3, .NET, and C# scripting.

![Whim demo](docs/images/demo.gif)

## Installation

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases) on GitHub. Whim has an [updater plugin](https://dalyisaac.github.io/Whim/docs/plugins/updater.html) to keep you up to date.

Installation via package managers is coming in [dalyIsaac/Whim#792](https://github.com/dalyIsaac/Whim/issues/792).

## Why use Whim?

A window manager is responsible for controlling the layout of windows in your desktop environment. Whim is a [dynamic window manager](https://en.wikipedia.org/wiki/Dynamic_window_manager), where windows are arranged according to different layouts.

Whim supports multiple layout engines. Each [workspace](https://dalyisaac.github.io/Whim/docs/customize/workspaces.html) can switch between different layout engines. For example, the `TreeLayoutEngine` allows users to create arbitrary grids of windows during runtime (similar to `i3`), while the `SliceLayoutEngine` fully automates windows placement using a predefined, customizable logic (similar to `Awesome` or `dwm`) . For more, see [Layout Engines](https://dalyisaac.github.io/Whim/docs/customize/layout-engines.html).

Whim is configured using C# scripting - no YAML to be found here. This means you can use the full power of C# to configure Whim. Whim also exposes its API for plugins to use. Accordingly, much of the more custom functionality has been implemented as plugins which users can choose to use or not.

Whim works by sitting on top of Windows' own window manager. It listens to window events and moves windows accordingly. Whim does not use Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors. Instead, Whim has [workspaces](https://dalyisaac.github.io/Whim/docs/customize/workspaces.html).

To see how Whim compares to other Windows window managers, see [Whim vs. Other Window Managers](https://dalyisaac.github.io/Whim/docs/getting-started/comparison.html).

## Documentation

You can find the Whim documentation at [dalyisaac.github.io/Whim](https://dalyisaac.github.io/Whim).

## Contributing

Whim is an open-source project and contributions are welcome! For more information, see the [Contributing](https://dalyisaac.github.io/Whim/docs/contribute/guide.html) page.

## Community

Whim has a [Discord server](https://discord.gg/gEFq9wr7jb) where you can ask questions, get help, and chat with other users.

## Thanks

Whim is heavily inspired by the [workspacer](https://github.com/workspacer/workspacer) project - for more, see the [Inspiration](https://dalyisaac.github.io/Whim/getting-started/inspiration.html) page.

Thank you to all the contributors to Whim for their help and support! 💖
