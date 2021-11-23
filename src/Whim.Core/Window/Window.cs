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
                    fixed (char* buffer = new char[_bufferCapacity])
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

        public event WindowUpdateDelegate? WindowUpdated;
        public event WindowFocusDelegate? WindowFocused;
        public event WindowDestroyDelegate? WindowDestroyed;

        public void BringToTop()
        {
            Logger.Debug($"Window.BringToTop: {Title}");
            PInvoke.BringWindowToTop(_handle);
        }

        public void Close()
        {
            Logger.Debug($"Window.Close: {Title}");
            Win32Helper.QuitApplication(_handle);
            WindowDestroyed?.Invoke(this);
        }

        public void Focus()
        {
            Logger.Debug($"Window.Focusing: {Title}");
            if (IsFocused)
            {
                Logger.Debug($"Window.Already focused: {Title}");
            }

            PInvoke.SetForegroundWindow(_handle);
            WindowFocused?.Invoke(this);
        }

        public void Hide()
        {
            Logger.Debug($"Window.Hide: {Title}");
            Win32Helper.HideWindow(_handle);
            WindowUpdated?.Invoke(this, WindowUpdateType.Hide);
        }

        public void ShowInCurrentState()
        {
            Logger.Debug($"Window.ShowInCurrentState: {Title}");
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
            Logger.Debug($"Window.ShowMaximized: {Title}");
            Win32Helper.ShowMaximizedWindow(_handle);
        }

        public void ShowMinimized()
        {
            Logger.Debug($"Window.ShowMinimized: {Title}");
            Win32Helper.ShowMinimizedWindow(_handle);
        }

        public void ShowNormal()
        {
            Logger.Debug($"Window.ShowNormal: {Title}");
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
            };
        }

        // This is scoped to internal until https://github.com/microsoft/CsWin32/issues/213 is resolved
        internal static Window? RegisterWindow(HWND handle, IWindowManager windowManager)
        {
            Logger.Debug($"Window.RegisterWindow: {handle}");

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

        internal void UnregisterWindow()
        {
            WindowDestroyed?.Invoke(this);
        }

        // NOTE: when writing docs, make a note that register and unregister are handled
        // separately here.
        public void HandleEvent(uint eventType)
        {
            switch (eventType)
            {
                // For cloaking, see https://devblogs.microsoft.com/oldnewthing/20200302-00/?p=103507
                case PInvoke.EVENT_OBJECT_CLOAKED:
                    UpdateWindow(WindowUpdateType.Hide);
                    break;
                case PInvoke.EVENT_OBJECT_UNCLOAKED:
                    UpdateWindow(WindowUpdateType.Show);
                    break;
                case PInvoke.EVENT_SYSTEM_MINIMIZESTART:
                    UpdateWindow(WindowUpdateType.MinimizeStart);
                    break;
                case PInvoke.EVENT_SYSTEM_MINIMIZEEND:
                    UpdateWindow(WindowUpdateType.MinimizeEnd);
                    break;
                case PInvoke.EVENT_SYSTEM_FOREGROUND:
                    UpdateWindow(WindowUpdateType.Foreground);
                    break;
                case PInvoke.EVENT_SYSTEM_MOVESIZESTART:
                    StartWindowMove();
                    break;
                case PInvoke.EVENT_SYSTEM_MOVESIZEEND:
                    EndWindowMove();
                    break;
                case PInvoke.EVENT_OBJECT_LOCATIONCHANGE:
                    WindowMove();
                    break;
            }
        }

        private void UpdateWindow(WindowUpdateType type)
        {
            WindowUpdated?.Invoke(this, type);
        }


        private void WindowMove()
        {
            // TODO: mouse handlers
        }

        private void EndWindowMove()
        {
            // TODO: mouse handlers
        }

        private void StartWindowMove()
        {
            // TODO: mouse handlers
        }
    }
}