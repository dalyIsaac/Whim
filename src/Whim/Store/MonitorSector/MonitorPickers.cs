using System.Collections.Generic;
using DotNext;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

public static partial class Pickers
{
	/// <summary>
	/// Get a monitor by its <see cref="HMONITOR"/> handle.
	/// </summary>
	/// <param name="handle"></param>
	/// <returns></returns>
	public static PurePicker<Result<IMonitor>> GetMonitorByHandle(HMONITOR handle) =>
		(rootSector) =>
		{
			foreach (IMonitor m in rootSector.MonitorSector.Monitors)
			{
				if (m.Handle == handle)
				{
					return Result.FromValue(m);
				}
			}

			return Result.FromException<IMonitor>(StoreExceptions.MonitorNotFound(handle));
		};

	/// <summary>
	/// Get the currently active monitor.
	/// </summary>
	public static PurePicker<IMonitor> GetActiveMonitor =>
		rootSector => GetMonitorByHandle(rootSector.MonitorSector.ActiveMonitorHandle)(rootSector).Value;

	/// <summary>
	/// Get the primary monitor.
	/// </summary>
	public static PurePicker<IMonitor> GetPrimaryMonitor =>
		rootSector => GetMonitorByHandle(rootSector.MonitorSector.PrimaryMonitorHandle)(rootSector).Value;

	/// <summary>
	/// Get the last <see cref="IMonitor"/> which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	public static PurePicker<IMonitor> GetLastWhimActiveMonitor =>
		rootSector => GetMonitorByHandle(rootSector.MonitorSector.LastWhimActiveMonitorHandle)(rootSector).Value;

	/// <summary>
	/// Get all the <see cref="IMonitor"/>s tracked by Whim.
	/// </summary>
	public static PurePicker<IEnumerable<IMonitor>> GetAllMonitors => rootSector => rootSector.MonitorSector.Monitors;
}
