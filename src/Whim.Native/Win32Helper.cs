using System;

namespace Whim.Native
{
    public static class Win32Helper
    {
        public static void QuitApplication(IntPtr hwnd)
        {
            Win32.SendNotifyMessage(hwnd, Win32.WM_SYSCOMMAND, Win32.SC_CLOSE, IntPtr.Zero);
        }


        public static void ForceForegroundWindow(IntPtr hwnd)
        {
            // Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
            Win32.keybd_event(0, 0, 0, UIntPtr.Zero);
            Win32.SetForegroundWindow(hwnd);
        }

        public static void HideWindow(IntPtr hwnd)
        {
            Win32.ShowWindow(hwnd, Win32.SW.SW_HIDE);
        }

        public static void ShowMaximizedWindow(IntPtr hwnd)
        {
            Win32.ShowWindow(hwnd, Win32.SW.SW_SHOWMAXIMIZED);
        }

        public static void ShowMinimizedWindow(IntPtr hwnd)
        {
            Win32.ShowWindow(hwnd, Win32.SW.SW_SHOWMINIMIZED);
        }

        public static void ShowNormalWindow(IntPtr hwnd)
        {
            // Shows the window in its most recent size and position.
            // Unlike SW_SHOWNORMAL, the window is not activated.
            Win32.ShowWindow(hwnd, Win32.SW.SW_SHOWNOACTIVATE);
        }

        public static void SetWindowsEventHook(Win32.EVENT_CONSTANTS eventMin, Win32.EVENT_CONSTANTS eventMax, WinEventDelegate lpfnWinEventProc)
        {
            Win32.SetWinEventHook(eventMin, eventMax, IntPtr.Zero, lpfnWinEventProc, 0, 0, Win32.WINEVENT_OUTOFCONTEXT);
        }
    }
}