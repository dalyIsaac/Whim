# Whim vs. Other Window Managers

## Overview

> [!NOTE]
> The table was last updated in January 2024.
>
> The information in this table represents a best-effort attempt to compare Whim to other window managers, but may be out of date and not be completely accurate.
> The information presented in the **Pros** and **Cons** sections are subjective.
>
> This table is based on the [Arch Linux Comparison of tiling window managers](https://wiki.archlinux.org/title/Comparison_of_tiling_window_managers) table.

| Feature\Window Manager | Whim                                                          | bug.n                               | FancyWM                                                                     | GlazeWM                      | Komorebi                                                                                                                                                                                                          | PowerToys' FancyZones | Workspacer                                                             |
| ---------------------- | ------------------------------------------------------------- | ----------------------------------- | --------------------------------------------------------------------------- | ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ---------------------------------------------------------------------- |
| Written in             | C#                                                            | AutoHotkey                          | C#                                                                          | C#                           | Rust 🦀                                                                                                                                                                                                           | C++                   | C#                                                                     |
| Configured with        | C#                                                            | AutoHotkey                          | GUI                                                                         | YAML                         | YAML or shell script                                                                                                                                                                                              | GUI                   | C#                                                                     |
| Multiple layouts       | ✅                                                            | ✅                                  | ❌, manual management                                                       | ❌, manual management        | ✅                                                                                                                                                                                                                | ✅                    | ✅                                                                     |
| Plugin architecture    | ✅ via C#                                                     | ❌                                  | ❌                                                                          | ✅-ish, via external control | ✅-ish, via external control                                                                                                                                                                                      | ❌                    | ✅ via C#                                                              |
| System tray support    | ❌ in [backlog](https://github.com/dalyIsaac/Whim/issues/78)  |                                     | ❌                                                                          | ✅                           | ❌                                                                                                                                                                                                                | ✅-ish                | ✅                                                                     |
| On the fly reload      | ❌                                                            | ❌                                  | ✅                                                                          | ✅                           | ✅                                                                                                                                                                                                                | ✅                    | ❌                                                                     |
| Information bars       | ✅                                                            | ✅                                  | ✅                                                                          | ✅                           | Via [Yasb](https://github.com/da-rth/yasb)                                                                                                                                                                        | ❌                    | ✅                                                                     |
| External control       | ❌ in [backlog](https://github.com/dalyIsaac/Whim/issues/670) | ❌                                  | ✅ via [shell scripting](https://github.com/FancyWM/fancywm/wiki/Scripting) | ❌                           | ✅ via [komorebic](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#configuration-with-komorebic), [named pipes and TCP](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#configuration-with-komorebic) | ❌                    | ❌                                                                     |
| Maintenance            | Active                                                        | In-active (last commit in Jan 2023) | Semi-active (last commit in Oct 2023)                                       | Active                       | Active                                                                                                                                                                                                            | Active                | [Not active](https://github.com/workspacer/workspacer/discussions/485) |
| License                | MIT                                                           | GPL-3.0                             | MIT                                                                         | GPL-3.0                      | MIT                                                                                                                                                                                                               | MIT                   | MIT                                                                    |

Definitions:

- **External control** refers to the ability to control the window manager from outside the window manager itself. For example, this could be done via a command line interface, or via a socket interface.
- **Dynamic management** emphasizes automatic management of window layouts for speed and simplicity.
- **Manual management** emphasizes manual adjustment of layout and sizing with potentially more precise control, at the cost of more time spent moving and sizing windows.

## bug.n

TODO

**Pros:**

TODO

**Cons:**

TODO

## FancyWM

TODO

**Pros:**

TODO

**Cons:**

TODO

## GlazeWM

TODO

**Pros:**

TODO

**Cons:**

TODO

## Komorebi

Komorebi is Rust 🦀-based window manager with a distinctly different philosophy than Whim.

> [Komorebi] is follows the sockets model of bspwm and yabai, which means that all config can be managed through AutoHotKey (or any other hotkey daemon of your choice) instead of trying to reinvent the wheel, and makes the TWM experience as scriptable as bspwm and yabai without increasing the complexity of the TWM codebase itself.

_[Written by the author of Komorebi](<(https://news.ycombinator.com/item?id=27427061)>) relating to Komorebi's predecessor, [yatta](https://github.com/LGUG2Z/yatta)._

**Pros:**

TODO

**Cons:**

TODO

## PowerToys' FancyZones

**Repository:** <https://github.com/microsoft/PowerToys/>

**Documentation:** <https://learn.microsoft.com/en-us/windows/powertoys/fancyzones>

FancyZones is a utility for arranging and snapping windows into sets of zone locations. It is _not_ a tiling window manager, in comparison to each of the other window managers in this comparison. The user must manually move windows into zones.

**Pros:**

TODO

**Cons:**

TODO

## Workspacer

Workspacer is the inspiration for Whim - see more at [Inspiration](concepts/inspiration.md).

**Pros:**

TODO

**Cons:**

TODO
