using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Whim.Native;

namespace Whim.Core.Window
{
    public class Window : IWindow
    {
        private readonly IntPtr _handle;

        public string Title
        {
            get
            {
                var buffer = new StringBuilder(255);
                Win32.GetWindowText(_handle, buffer, buffer.Capacity + 1);
                return buffer.ToString();
            }
        }

        public string Class
        {
            get
            {
                var buffer = new StringBuilder(255);
                Win32.GetClassName(_handle, buffer, buffer.Capacity + 1);
                return buffer.ToString();
            }
        }

        public IWindowLocation Location
        {
            get
            {
                Win32.Rect rect = new Win32.Rect();
                Win32.GetWindowRect(_handle, ref rect);

                WindowState state = WindowState.Normal;
                if (IsMinimized)
                {
                    state = WindowState.Minimized;
                }
                else if (IsMaximized)
                {
                    state = WindowState.Maximized;
                }

                return new WindowLocation(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, state);
            }
        }

        public int ProcessId { get; }

        public string ProcessFileName { get; }

        public string ProcessName { get; }

        public bool IsFocused => Win32.GetForegroundWindow() == _handle;

        public bool IsMinimized => Win32.IsIconic(_handle);

        public bool IsMaximized => Win32.IsZoomed(_handle);

        public bool IsMouseMoving { get; internal set; }

        public IWindowManager WindowManager { get; }

        public event WindowAddDelegate WindowAdded;
        public event WindowUpdateDelegate WindowUpdated;
        public event WindowFocusDelegate WindowFocused;
        public event WindowDestroyDelegate WindowDestroyed;

        public void BringToTop()
        {
            Win32.BringWindowToTop(_handle);
        }

        public void Close()
        {
            Win32Helper.QuitApplication(_handle);
        }

        public void Focus()
        {
            if (!IsFocused)
            {
                Win32.SetForegroundWindow(_handle);
                WindowFocused?.Invoke(this);
            }
        }

        public void Hide()
        {
            Win32Helper.HideWindow(_handle);
        }

        public void ShowInCurrentState()
        {
            if (IsMinimized)
            {
                ShowMinimized();
            }
            else if (IsMaximized)
            {
                ShowMaximized();
            }
            else
            {
                ShowNormal();
            }
        }

        public void ShowMaximized()
        {
            Win32Helper.ShowMaximizedWindow(_handle);
        }

        public void ShowMinimized()
        {
            Win32Helper.ShowMinimizedWindow(_handle);
        }

        public void ShowNormal()
        {
            Win32Helper.ShowNormalWindow(_handle);
        }

        private Window(IntPtr handle, IWindowManager windowManager)
        {
            _handle = handle;
            WindowManager = windowManager;

            uint pid;
            Win32.GetWindowThreadProcessId(_handle, out pid);
            ProcessId = (int)pid;

            var process = Process.GetProcessById(ProcessId);
            ProcessName = process.ProcessName;

            try
            {
                ProcessFileName = Path.GetFileName(process.MainModule?.FileName) ?? "--NA--";
            }
            catch (Win32Exception)
            {
                // Win32Exception is thrown when it's not possible to get
                // information about the process. See
                // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0#remarks
                ProcessFileName = "--NA--";
            }
        }

        public static Window? RegisterWindow(IntPtr handle, IWindowManager windowManager)
        {
            if (handle == IntPtr.Zero)
                return null;

            try
            {
                return new Window(handle, windowManager);

            }
            catch (Exception e)
            {
                Logger.Error(e, "could not create a Window instance");
                return null;
            }
        }
    }
}