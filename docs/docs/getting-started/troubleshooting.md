# Troubleshooting

## Managing troublesome windows

Some applications are difficult to manage. Whim will try to manage them as best as it can, but there are some limitations. For example, Firefox will try to remember its size and position, and will fight against Whim's attempts to manage it on first load.

To get around this, Whim has <xref:Whim.IWindowProcessor>s. These are used to tell Whim to ignore specific window messages (see [Event Constants](https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants)). For example, the <xref:Whim.FirefoxWindowProcessor> ignores all events until the first [`EVENT_OBJECT_CLOAKED`](https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants#:~:text=EVENT_OBJECT_CLOAKED) event is received.

## Window launch locations

Windows can launch windows in different locations. Additionally, interacting with some untracked windows like the Windows Taskbar can break focus tracking in Whim.

To counteract this, the <xref:Whim.IRouterManager> has a <xref:Whim.IRouterManager.RouterOptions> property which can configure how new windows are routed - see the <xref:Whim.RouterOptions> enum.

## Adding/removing monitors

When adding and removing monitors, Windows will very helpfully move windows between monitors. However, this conflicts with Whim. To work around Windows' helpfulness, Whim (in the `WindowManager` and `ButlerEventHandlers`) will ignore [`WinEvents`](../architecture/events.md) for 3 seconds for tracked windows. After the 3 seconds have elapsed, Whim will lay out all the active workspaces.

## Window overflows given area

Whim will request windows to have a specific size, but some windows (like Spotify) enforce a minimum size and will ignore Whim's instructions. Whim does not account for this. As a result, these stubborn windows will overflow the expected area. This will also result in the [focus indicator](../plugins/focus-indicator.md) highlighting the expected area, not the window's actual area.
