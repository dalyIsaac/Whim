using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Whim.Core.Monitor
{
    public class MonitorManager : IMonitorManager
    {
        public Commander Commander { get; } = new();
        private Monitor[] _monitors;
        public IMonitor FocusedMonitor { get; private set; }
        public int Length => _monitors.Length;

        public IEnumerator<IMonitor> GetEnumerator() => (IEnumerator<IMonitor>)_monitors.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///
        /// </summary>
        /// <exception cref="Exception">When no monitors are found, or there is no primary monitor.</exception>
        public MonitorManager()
        {
            var screens = Screen.AllScreens;

            _monitors = new Monitor[screens.Length];
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                if (screen.Primary)
                {
                    FocusedMonitor = new Monitor(screen);
                }

                _monitors[i] = new Monitor(screen);
            }

            if (_monitors.Length == 0)
            {
                throw new Exception("No monitors were found");
            }
            if (FocusedMonitor == null)
            {
                throw new Exception("Failed to find primary monitor");
            }
        }

        public IMonitor GetMonitorAtPoint(int x, int y)
        {
            var screen = Screen.FromPoint(new System.Drawing.Point(x, y));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }
    }
}