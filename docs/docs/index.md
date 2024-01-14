# Introduction

> All the power to the user!

Whim is an hackable, pluggable and scriptable dynamic window manager for Windows 10 and 11, built using WinUI 3, .NET, and C# scripting.

![Whim demo](/images/demo.gif)

## Why use Whim?

A window manager is responsible for controlling the layout of windows in your desktop environment. Whim is a [dynamic window manager](https://en.wikipedia.org/wiki/Dynamic_window_manager), where windows are arranged according to different layouts.

Whim supports multiple layout engines. Each [workspace](concepts/workspaces.md) can switch between different layout engines. For example, the `TreeLayoutEngine` allows users to create arbitrary grids of windows, while the `FocusLayoutEngine` allows users to focus on a single window at a time. For more, see [Layout Engines](layout-engines.md).

Whim is configured using C# scripting - no YAML to be found here. This means you can use the full power of C# to configure Whim. Whim also exposes its API for plugins to use. Accordingly, much of the more custom functionality has been implemented as plugins which users can choose to use or not.

Whim works by sitting on top of Windows' own window manager. It listens to window events and moves windows accordingly.

To see how Whim compares to other Windows window managers, see [Whim vs. Other Window Managers](comparison.md).

## Installation

Alpha builds are available on the [releases page](https://github.com/dalyIsaac/Whim/releases) at GitHub.
