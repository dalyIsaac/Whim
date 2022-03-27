using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class MonitorInfo
{
	[DllImport("User32", ExactSpelling = true, EntryPoint = "GetMonitorInfoW")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)] private static extern unsafe bool GetMonitorInfo(HMONITOR hMonitor, MONITORINFOEXW* lpmi);

	internal static unsafe bool GetMonitorInfo(HMONITOR monitor, ref MONITORINFOEXW lpmi)
	{
		fixed (MONITORINFOEXW* lmpiLocal = &lpmi)
		{
			return GetMonitorInfo(monitor, lmpiLocal);
		}
	}

	internal static ILocation<int> GetLocation(this MONITORINFOEXW monitor)
	{
		return monitor.__AnonymousBase_winuser_L13571_C43.rcMonitor.ToLocation();
	}

	internal static string GetDeviceName(this MONITORINFOEXW monitor)
	{
		return monitor.szDevice.ToString();
	}

	internal static bool IsPrimary(this MONITORINFOEXW monitor)
	{
		return (monitor.__AnonymousBase_winuser_L13571_C43.dwFlags & (uint)MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY) != 0;
	}
}
