using System;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class StoreExceptions
{
	public static Exception MonitorNotFound(HMONITOR handle) =>
		new WhimException($"Monitor with handle {handle} not found.");

	public static Exception NoMonitorFoundAtPoint(IPoint<int> point) =>
		new WhimException($"No monitor found at point {point}.");
}
