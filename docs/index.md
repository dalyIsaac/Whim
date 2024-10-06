# Introduction

<!-- NOTE: This is largely duplicated to the README.md -->

> All the power to the user!

Whim is a hackable, pluggable and scriptable dynamic window manager for Windows 10 and 11, built using WinUI 3, .NET, and C# scripting. Whim supports multiple layout engines, workspaces, and plugins. Whim is designed to be highly customizable and extensible, allowing users to create their own layouts, workspaces, and plugins.

Whim can be configured using YAML, JSON, or C# scripting.

![Whim demo](images/demo.gif)

## Installation

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases) on GitHub. Whim has an [updater plugin](configure/plugins/updater.md) to notify you of new releases.

Installation via package managers is coming in [dalyIsaac/Whim#792](https://github.com/dalyIsaac/Whim/issues/792).

## Why use Whim?

A window manager is responsible for controlling the layout of windows in your desktop environment. Whim is a [dynamic window manager](https://en.wikipedia.org/wiki/Dynamic_window_manager), where windows are arranged according to different layouts.

Whim supports multiple layout engines. Each [workspace](configure/core/workspaces.md) can switch between different layout engines. For example, the [`TreeLayoutEngine`](configure/core/layout-engines.md#treelayoutengine) allows users to create arbitrary grids of windows during runtime (similar to `i3`), while the [`SliceLayoutEngine`](configure/core/layout-engines.md#slicelayoutengine) fully automates windows placement using a predefined, customizable logic (similar to `Awesome` or `dwm`) . For more, see [Layout Engines](configure/core/layout-engines.md).

Whim is configured using C# scripting - no YAML to be found here. This means you can use the full power of C# to configure Whim. Whim also exposes its API for plugins to use. Accordingly, much of the more custom functionality has been implemented as plugins which users can choose to use or not.

Whim works by sitting on top of Windows' own window manager. It listens to window events and moves windows accordingly. Whim does not use Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors. Instead, Whim has [workspaces](configure/core/workspaces.md).

To see how Whim compares to other Windows window managers, see [Whim vs. Other Window Managers](intro/comparison.md).
