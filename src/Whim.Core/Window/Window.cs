using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Whim.Core.Window
{
    public class Window : IWindow
    {
        private const int _bufferCapacity = 255;
        private readonly HWND _handle;

        public string Title
        {
            get
            {
                unsafe
                {
                    fixed (char* buffer = new char[_bufferCapacity])
                    {
                        PInvoke.GetWindowText(_handle, buffer, _bufferCapacity + 1);
                        return new string(buffer);
                    }
                }
            }
        }

        public string Class
        {
            get
            {
                unsafe
                {
                    fixed(char* buffer = new char[_bufferCapacity])
                    {
                        PInvoke.GetClassName(_handle, buffer, _bufferCapacity + 1);
                        return new string(buffer);
                    }
                }
            }
        }

        public IWindowLocation Location
        {
            get
            {
                PInvoke.GetWindowRect(_handle, out RECT rect);

                WindowState state = WindowState.Normal;
                if (IsMinimized)
                {
                    state = WindowState.Minimized;
                }
                else if (IsMaximized)
                {
                    state = WindowState.Maximized;
                }

                return new WindowLocation(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, state);
            }
        }

        public int ProcessId { get; }

        public string ProcessFileName { get; }

        public string ProcessName { get; }

        public bool IsFocused => PInvoke.GetForegroundWindow() == _handle;

        public bool IsMinimized => PInvoke.IsIconic(_handle);

        public bool IsMaximized => PInvoke.IsZoomed(_handle);

        public bool IsMouseMoving { get; internal set; }

        public IWindowManager WindowManager { get; }

        public event WindowAddDelegate WindowAdded;
        public event WindowUpdateDelegate WindowUpdated;
        public event WindowFocusDelegate WindowFocused;
        public event WindowDestroyDelegate WindowDestroyed;

        public void BringToTop()
        {
            PInvoke.BringWindowToTop(_handle);
        }

        public void Close()
        {
            Win32Helper.QuitApplication(_handle);
        }

        public void Focus()
        {
            if (!IsFocused)
            {
                PInvoke.SetForegroundWindow(_handle);
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

        private Window(HWND handle, IWindowManager windowManager)
        {
            _handle = handle;
            WindowManager = windowManager;

            unsafe
            {
                uint pid;
                PInvoke.GetWindowThreadProcessId(_handle, &pid);
                ProcessId = (int)pid;
            }

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

        private static Window? RegisterWindow(HWND handle, IWindowManager windowManager)
        {
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