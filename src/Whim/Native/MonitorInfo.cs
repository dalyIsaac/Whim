using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class MonitorInfo
{
	internal static string GetDeviceName(this MONITORINFOEXW monitor)
	{
		return monitor.szDevice.ToString();
	}

	internal static bool IsPrimary(this MONITORINFOEXW monitor)
	{
		return (monitor.monitorInfo.dwFlags & (uint)MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY) != 0;
	}
}
