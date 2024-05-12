using System;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class StoreExceptions
{
	public static Exception MonitorNotFound(HMONITOR handle) =>
		new WhimException($"Monitor with handle {handle} not found.");
}
