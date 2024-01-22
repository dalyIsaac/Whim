# Whim vs. Other Window Managers

## Overview

> [!NOTE]
> The table was last updated in January 2024.
>
> The information in this table represents a best-effort attempt to compare Whim to other window managers, but may be out of date and not be completely accurate.
>
> This table is based on the [Arch Linux Comparison of tiling window managers](https://wiki.archlinux.org/title/Comparison_of_tiling_window_managers) table.

| Feature\Window Manager | Whim                                                           | bug.n                                                                          | FancyWM                                                                      | GlazeWM                       | Komorebi                                                                                                                                                                                                            | PowerToys' FancyZones | Workspacer                                                             |
| ---------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------------- | ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ---------------------------------------------------------------------- |
| Written in             | C#                                                             | AutoHotkey                                                                     | C#                                                                           | C#                            | Rust 🦀                                                                                                                                                                                                             | C++                   | C#                                                                     |
| Configured with        | C#                                                             | AutoHotkey                                                                     | GUI                                                                          | YAML                          | YAML or shell script                                                                                                                                                                                                | GUI                   | C#                                                                     |
| Multiple layouts       | Yes                                                            | Yes                                                                            | No, manual management                                                        | No, manual management         | Yes                                                                                                                                                                                                                 | Yes                   | Yes                                                                    |
| Plugin architecture    | Yes via C#                                                     | No                                                                             | No                                                                           | Yes-ish, via external control | Yes-ish, via external control                                                                                                                                                                                       | No                    | Yes via C#                                                             |
| System tray support    | No, in [backlog](https://github.com/dalyIsaac/Whim/issues/78)  | No                                                                             | No                                                                           | Yes                           | No                                                                                                                                                                                                                  | Yes-ish               | Yes                                                                    |
| On the fly reload      | No                                                             | No                                                                             | Yes                                                                          | Yes                           | Yes                                                                                                                                                                                                                 | Yes                   | No                                                                     |
| Information bars       | Yes                                                            | Yes                                                                            | Yes                                                                          | Yes                           | Yes, via [Yasb](https://github.com/da-rth/yasb)                                                                                                                                                                     | No                    | Yes                                                                    |
| External control       | No, in [backlog](https://github.com/dalyIsaac/Whim/issues/670) | No                                                                             | Yes via [shell scripting](https://github.com/FancyWM/fancywm/wiki/Scripting) | No                            | Yes, via [komorebic](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#configuration-with-komorebic), [named pipes and TCP](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#configuration-with-komorebic) | No                    | No                                                                     |
| Maintenance            | Active                                                         | [Not active](https://github.com/fuhsjr00/bug.n?tab=readme-ov-file#development) | Semi-active (~Oct 2023)                                                      | Active                        | Active                                                                                                                                                                                                              | Active                | [Not active](https://github.com/workspacer/workspacer/discussions/485) |
| License                | MIT                                                            | GPL-3.0                                                                        | MIT                                                                          | GPL-3.0                       | MIT                                                                                                                                                                                                                 | MIT                   | MIT                                                                    |

Definitions:

- **External control** refers to the ability to control the window manager from outside the window manager itself. For example, this could be done via a command line interface, or via a socket interface.
- **Dynamic management** emphasizes automatic management of window layouts for speed and simplicity.
- **Manual management** emphasizes manual adjustment of layout and sizing with potentially more precise control, at the cost of more time spent moving and sizing windows.

> [!WARNING]
> Some of the information presented in following sections is subjective.

## bug.n

> **Repository:** <https://github.com/fuhsjr00/bug.n>
>
> **Documentation:** <https://github.com/fuhsjr00/bug.n/wiki>

TODO

## FancyWM

> **Repository:** <https://github.com/FancyWM/fancywm>
>
> **Documentation:** <https://github.com/FancyWM/fancywm/wiki>

TODO

## GlazeWM

> **Repository:** <https://github.com/glzr-io/glazewm>
>
> **Documentation:** <https://github.com/glzr-io/glazewm/blob/main/README.md>

TODO

<!-- - Single executable
- YAML configuration is not as flexible as C# configuration -->

## Komorebi

> **Repository:** <https://github.com/LGUG2Z/komorebi>
>
> **Documentation:** <https://github.com/LGUG2Z/komorebi/blob/master/README.md>

Komorebi is Rust 🦀-based window manager with a distinctly different philosophy than Whim.

> [Komorebi] is follows the sockets model of bspwm and yabai, which means that all config can be managed through AutoHotKey (or any other hotkey daemon of your choice) instead of trying to reinvent the wheel, and makes the TWM experience as scriptable as bspwm and yabai without increasing the complexity of the TWM codebase itself.

_[Written by the author of Komorebi](<(https://news.ycombinator.com/item?id=27427061)>) relating to Komorebi's predecessor, [yatta](https://github.com/LGUG2Z/yatta)._

TODO

<!-- **Pros:**

- Written in Rust (performance, no garbage collection - I like Rust 🦀)
- The socket model is elegant and works well with other tools
- Komorebi has a [community-driven repository](https://github.com/LGUG2Z/komorebi-application-specific-configuration) for application-specific configuration, to deal with the various quirks of different applications - Whim currently consumes a subset of this repository as part of its core rules
- Single executable

**Cons:**

- Requires YAML and/or the knowledge of an external scripting language to configure -->

## PowerToys' FancyZones

> **Repository:** <https://github.com/microsoft/PowerToys>
>
> **Documentation:** <https://learn.microsoft.com/en-us/windows/powertoys/fancyzones>

FancyZones is a utility for arranging and snapping windows into sets of zone locations. It is _not_ a tiling window manager, in comparison to each of the other window managers in this comparison. The user must manually move windows into zones.

TODO

<!-- **Pros:**

- Easy to configure

**Cons:**

- Moving windows into zones is manual and tedious
- No automatic tiling or layout management -->

## Workspacer

> **Repository:** <https://github.com/workspacer/workspacer>
>
> **Documentation:** <https://workspacer.org>

Workspacer is the inspiration for Whim - see more at [Inspiration](concepts/inspiration.md).

TODO

<!-- **Pros:**

TODO

**Cons:**

- Not actively maintained
- Layout engines are _all_ stack-based, limiting the flexibility of the window manager -->
