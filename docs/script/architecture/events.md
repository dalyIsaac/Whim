# Events

There are a variety of ways that Whim receives events:

| Event Source                     | User                             | Description                                                                                                                                                   |
| -------------------------------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `WindowEventListener`            | `WindowSector` transforms        | Receives [`WinEvents`](https://learn.microsoft.com/en-us/windows/win32/winauto/what-are-winevents) and dispatch appropriate transforms for the `WindowSector` |
| `WindowMessageMonitor`           | `MonitorEventListener`           | Receives `WM_MESSAGE` events in a dedicated Whim window whose only purpose is to receive said events                                                          |
| `MonitorEventListener`           | `MonitorSector` transforms       | Receives events from `WindowMessageMonitor` and dispatch appropriate transforms for the `MonitorSector`                                                       |
| `KeybindHook`                    | <xref:Whim.IKeybindManager>      | Receives all keyboard events from `WH_KEYBOARD_LL`                                                                                                            |
| `MouseHook`                      | `WindowSector` transforms        | Receives all mouse events from `WH_MOUSE_LL`                                                                                                                  |
| <xref:Whim.INotificationManager> | <xref:Whim.INotificationManager> | Receives events from user interactions with Windows events                                                                                                    |
| WinUI elements                   | WinUI elements                   | Receives user interactions with visual elements                                                                                                               |

Each of these entrypoints are wrapped with <xref:Whim.IContext.HandleUncaughtException(System.String,System.Exception)>. The behavior of this wrapper method can be customized using <Whim.IContext.UncaughtExceptionHandling>.
