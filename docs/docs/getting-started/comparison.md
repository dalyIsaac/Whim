# Whim vs. other window managers

## Comparison table

> [!NOTE]
> The table was last updated in January 2024.
>
> The information in this table represents a best-effort attempt to compare Whim to other window managers, but may be out of date and not be completely accurate.
>
> This table is based on the [Arch Linux Comparison of tiling window managers](https://wiki.archlinux.org/title/Comparison_of_tiling_window_managers) table.

| Symbol | Meaning               |
| ------ | --------------------- |
| ✅     | Yes                   |
| ☑️     | Yes, but with caveats |
| 💡     | Planned               |
| ❌     | No                    |

| Feature\Window Manager | Whim                                                          | bug.n                                                                          | FancyWM                                                                         | GlazeWM                 | Komorebi                                                                                                                                                                                                           | PowerToys' FancyZones                                                                                                    | Workspacer                                                             |
| ---------------------- | ------------------------------------------------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------- | ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------- |
| Written in             | C#                                                            | AutoHotkey                                                                     | C#                                                                              | C#                      | Rust 🦀                                                                                                                                                                                                            | C++                                                                                                                      | C#                                                                     |
| Configured with        | C#                                                            | AutoHotkey                                                                     | GUI/JSON                                                                        | YAML                    | YAML or shell script                                                                                                                                                                                               | GUI                                                                                                                      | C#                                                                     |
| Dynamic management     | ✅                                                            | ✅                                                                             | ✅                                                                              | ❌                      | ✅                                                                                                                                                                                                                 | ❌                                                                                                                       | ✅                                                                     |
| Manual management      | ✅                                                            | ❌                                                                             | ❌                                                                              | ✅                      | ❌                                                                                                                                                                                                                 | ☑️ but not tiling                                                                                                        | ❌                                                                     |
| Customizable layouts   | ✅                                                            | ❌                                                                             | ☑️ with [limited manual panels](https://github.com/FancyWM/fancywm/wiki/Panels) | ❌                      | ☑️ with [custom dynamic layouts](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#creating-and-loading-custom-layouts)                                                                                        | ☑️ with [custom manual layouts](https://learn.microsoft.com/en-us/windows/powertoys/fancyzones#creating-a-custom-layout) | ✅                                                                     |
| Plugin architecture    | ✅ via C#                                                     | ❌                                                                             | ❌                                                                              | ☑️ via external control | ☑️ via external control                                                                                                                                                                                            | ❌                                                                                                                       | ✅ via C#                                                              |
| System tray support    | 💡 in [backlog](https://github.com/dalyIsaac/Whim/issues/78)  | ❌                                                                             | ❌                                                                              | ✅                      | ❌                                                                                                                                                                                                                 | ❌                                                                                                                       | ✅                                                                     |
| On the fly reload      | ❌                                                            | ❌                                                                             | ❌ - see [settings.json](https://github.com/FancyWM/fancywm/wiki/settings.json) | ✅                      | ✅                                                                                                                                                                                                                 | ✅                                                                                                                       | ❌                                                                     |
| Information bars       | ✅                                                            | ✅                                                                             | ✅                                                                              | ✅                      | ✅, via [Yasb](https://github.com/da-rth/yasb)                                                                                                                                                                     | ❌                                                                                                                       | ✅                                                                     |
| External control       | 💡 in [backlog](https://github.com/dalyIsaac/Whim/issues/670) | ❌                                                                             | ✅ via [shell scripting](https://github.com/FancyWM/fancywm/wiki/Scripting)     | ❌                      | ✅, via [komorebic](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#configuration-with-komorebic), [named pipes and TCP](https://github.com/LGUG2Z/komorebi?tab=readme-ov-file#configuration-with-komorebic) | ❌                                                                                                                       | ❌                                                                     |
| Maintenance            | Active                                                        | [Not active](https://github.com/fuhsjr00/bug.n?tab=readme-ov-file#development) | Semi-active (~Oct 2023)                                                         | Active                  | Active                                                                                                                                                                                                             | Active                                                                                                                   | [Not active](https://github.com/workspacer/workspacer/discussions/485) |
| License                | MIT                                                           | GPL-3.0                                                                        | MIT                                                                             | GPL-3.0                 | MIT                                                                                                                                                                                                                | MIT                                                                                                                      | MIT                                                                    |

Definitions:

- **External control** refers to the ability to control the window manager from outside the window manager itself. For example, this could be done via a command line interface, or via a socket interface.
- **Dynamic management** emphasizes automatic management of window layouts for speed and simplicity.
- **Manual management** (i3-style) emphasizes manual adjustment of layout and sizing with potentially more precise control, at the cost of more time spent moving and sizing windows.

Whim is the only window manager which is capable of both dynamic and manual management, with the ability to add custom layouts.

## Comparison notes

> [!WARNING]
> Some of the information presented in following sections is subjective.

## bug.n

[Repository](https://github.com/fuhsjr00/bug.n) | [Documentation](https://github.com/fuhsjr00/bug.n/wiki)

bug.n is an interesting idea, especially for users who already use AutoHotKey. Unfortunately, bug.n is no longer actively developed or maintained.

## FancyWM

[Repository](https://github.com/FancyWM/fancywm) | [Documentation](https://github.com/FancyWM/fancywm/wiki)

FancyWM calls itself a "dynamic tiling window manager", with what appears to be some a limited capability for manual management via [panels](https://github.com/FancyWM/fancywm/wiki/Panels#embedded-panels). It has an interesting capability called ["Windows Actions Dropdowns"](https://github.com/FancyWM/fancywm/wiki/Window-Actions-Dropdown) for customizing window behavior.

## GlazeWM

[Repository](https://github.com/glzr-io/glazewm) | [Documentation](https://github.com/glzr-io/glazewm/blob/main/README.md)

GlazeWM is in very active development, and has the ability to run from a single executable without requiring installation. It currently has a richer set of bar components compared to Whim.

## Komorebi

[Repository](https://github.com/LGUG2Z/komorebi) | [Documentation](https://github.com/LGUG2Z/komorebi/blob/master/README.md)

Komorebi is Rust 🦀-based window manager with a distinctly different philosophy than Whim.

> [Komorebi] follows the sockets model of bspwm and yabai, which means that all config can be managed through AutoHotKey (or any other hotkey daemon of your choice) instead of trying to reinvent the wheel, and makes the TWM experience as scriptable as bspwm and yabai without increasing the complexity of the TWM codebase itself.

_[Written by the author of Komorebi](https://news.ycombinator.com/item?id=27427061) relating to Komorebi's predecessor, [yatta](https://github.com/LGUG2Z/yatta)._

Komorebi is written in Rust 🦀, and has an elegant socket model which works well with other tools. However, it requires significant use of YAML or an external scripting language to configure. Komorebi has a [community-driven repository](https://github.com/LGUG2Z/komorebi-application-specific-configuration) for application-specific configuration, to deal with the various quirks of different applications - Whim currently consumes a subset of this repository as part of its core rules

## PowerToys' FancyZones

[Repository](https://github.com/microsoft/PowerToys) | [Documentation](https://learn.microsoft.com/en-us/windows/powertoys/fancyzones)

FancyZones is a utility for arranging and snapping windows into sets of zone locations. It is _not_ a tiling window manager, in comparison to each of the other window managers in this comparison. The user must manually move windows into zones. It does support custom layouts, but again, these do not apply any automatic tiling.

## Workspacer

[Repository](https://github.com/workspacer/workspacer) | [Documentation](https://workspacer.org)

Workspacer is the inspiration for Whim - see more at [Inspiration](./inspiration.md).
