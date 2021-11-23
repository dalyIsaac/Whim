using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Accessibility;

namespace Whim.Core
{
    // This is scoped to internal until https://github.com/microsoft/CsWin32/issues/213 is resolved
    internal static class Win32Helper
    {
        internal static void QuitApplication(HWND hwnd)
        {
            PInvoke.SendNotifyMessage(hwnd, PInvoke.WM_SYSCOMMAND, new WPARAM(PInvoke.SC_CLOSE), 0);
        }


        internal static void ForceForegroundWindow(HWND hwnd)
        {
            // Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
            PInvoke.keybd_event(0, 0, 0, 0);
            PInvoke.SetForegroundWindow(hwnd);
        }

        internal static void HideWindow(HWND hwnd)
        {
            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_HIDE);
        }

        internal static void ShowMaximizedWindow(HWND hwnd)
        {
            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
        }

        internal static void ShowMinimizedWindow(HWND hwnd)
        {
            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMINIMIZED);
        }

        internal static void ShowNormalWindow(HWND hwnd)
        {
            // Shows the window in its most recent size and position.
            // Unlike SW_SHOWNORMAL, the window is not activated.
            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
        }

        internal static UnhookWinEventSafeHandle SetWindowsEventHook(uint eventMin, uint eventMax, WINEVENTPROC lpfnWinEventProc)
            => PInvoke.SetWinEventHook(eventMin, eventMax, null, lpfnWinEventProc, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);
    }
}