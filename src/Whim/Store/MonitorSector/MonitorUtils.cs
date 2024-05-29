using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class MonitorUtils
{
	/// <summary>
	/// Gets all the current monitors.
	/// </summary>
	/// <param name="internalCtx"></param>
	/// <returns></returns>
	/// <exception cref="Exception">When no monitors are found.</exception>
	public static unsafe ImmutableArray<IMonitor> GetCurrentMonitors(IInternalContext internalCtx)
	{
		List<HMONITOR> hmonitors = new();
		HMONITOR primaryHMonitor = internalCtx.CoreNativeManager.MonitorFromPoint(
			new Point(0, 0),
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY
		);

		if (internalCtx.CoreNativeManager.HasMultipleMonitors())
		{
			MonitorEnumCallback closure = new();
			MONITORENUMPROC proc = new(closure.Callback);

			internalCtx.CoreNativeManager.EnumDisplayMonitors(null, null, proc, (LPARAM)0);
			hmonitors = closure.Monitors;
		}

		if (hmonitors.Count == 0)
		{
			hmonitors.Add(primaryHMonitor);
		}

		IMonitor[] currentMonitors = new Monitor[hmonitors.Count];
		for (int i = 0; i < currentMonitors.Length; i++)
		{
			HMONITOR hmonitor = hmonitors[i];
			bool isPrimaryHMonitor = hmonitor == primaryHMonitor;

			currentMonitors[i] = new Monitor(internalCtx, hmonitor, isPrimaryHMonitor);
		}

		return currentMonitors.OrderBy(m => m.WorkingArea.X).ThenBy(m => m.WorkingArea.Y).ToArray().ToImmutableArray();
	}

	public static HMONITOR OrActiveMonitor(this HMONITOR handle, IRootSector rootSector) =>
		handle == default ? rootSector.MonitorSector.ActiveMonitorHandle : handle;
}
