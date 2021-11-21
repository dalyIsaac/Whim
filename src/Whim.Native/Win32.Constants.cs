using System;

namespace Whim.Native
{
    public static partial class Win32
    {

        #region  WM_SYSCOMMAND
        public const uint WM_SYSCOMMAND = 0x0112;

        public enum SysCommands
        {
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            SC_RESTORE = 0xF120,
            SC_CLOSE = 0xF060,
        }
        #endregion

        public enum SW
        {
            SW_FORCEMINIMIZE = 11,
            SW_SHOW = 5,
            SW_SHOWNA = 8,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWMAXIMIZED = 3,
            SW_HIDE = 0,
            SW_RESTORE = 9,
            SW_MINIMIZE = 6,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWNORMAL = 1
        }
    }
}