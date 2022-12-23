using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal class MonitorEnumCallback
	{
		public List<HMONITOR> Monitors { get; } = new();

		public unsafe BOOL Callback(HMONITOR monitor, HDC hdc, RECT* rect, LPARAM param)
		{
			Monitors.Add(monitor);
			return (BOOL)true;
		}
	}
