using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class MonitorInfo
{
	internal static unsafe bool GetMonitorInfo(HMONITOR monitor, ref MONITORINFOEXW lpmi)
	{
		fixed (MONITORINFOEXW* lmpiLocal = &lpmi)
		{
			return PInvoke.GetMonitorInfo(monitor, (MONITORINFO*)lmpiLocal);
		}
	}

	internal static ILocation<int> GetLocation(this MONITORINFOEXW monitor)
	{
		return monitor.monitorInfo.rcMonitor.ToLocation();
	}

	internal static string GetDeviceName(this MONITORINFOEXW monitor)
	{
		return monitor.szDevice.ToString();
	}

	internal static bool IsPrimary(this MONITORINFOEXW monitor)
	{
		return (monitor.monitorInfo.dwFlags & (uint)MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY) != 0;
	}
}
