# Events

There are a variety of ways that Whim receives events:

- `IWindowMessageMonitor`, to receive `WM_MESSAGE` events in a dedicated Whim window whose only purpose is to receive said events
- `IKeybindHook` to receive all keyboard events from `WH_KEYBOARD_LL`
- `IMouseHook`, to receive all mouse events from `WH_MOUSE_LL`
- `INotificationManager`, to receive events from user interactions with Windows events
- WinUI elements like windows, to receive user interactions with visual elements

Each of these entrypoints are wrapped with <xref:Whim.IContext.HandleUncaughtException(System.String,System.Exception)>. The behavior of this wrapper method can be customized using <Whim.IContext.UncaughtExceptionHandling>.
