using System.Collections.Generic;
using System.Collections.Immutable;
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
	public static PurePicker<IMonitor> GetActiveMonitor() =>
		static (rootSector) => GetMonitorByHandle(rootSector.MonitorSector.ActiveMonitorHandle)(rootSector).Value;

	/// <summary>
	/// Get the primary monitor.
	/// </summary>
	public static PurePicker<IMonitor> GetPrimaryMonitor() =>
		static (rootSector) => GetMonitorByHandle(rootSector.MonitorSector.PrimaryMonitorHandle)(rootSector).Value;

	/// <summary>
	/// Get the last <see cref="IMonitor"/> which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	public static PurePicker<IMonitor> GetLastWhimActiveMonitor() =>
		static (rootSector) =>
			GetMonitorByHandle(rootSector.MonitorSector.LastWhimActiveMonitorHandle)(rootSector).Value;

	/// <summary>
	/// Get all the <see cref="IMonitor"/>s tracked by Whim.
	/// </summary>
	public static PurePicker<IReadOnlyList<IMonitor>> GetAllMonitors() =>
		static (rootSector) => rootSector.MonitorSector.Monitors;

	/// <summary>
	/// Gets the monitor before the given monitor.
	/// </summary>
	/// <param name="handle">
	/// The handle of the monitor to use as a reference. Defaults to the current active monitor.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, gets the previous monitor, otherwise gets the next monitor. Defaults to <see langword="true" />.
	/// </param>
	/// <param name="getFirst">
	/// When <see langword="true"/>, then returns the first monitor. Otherwise returns an exception in the
	/// result.
	/// </param>
	/// <returns></returns>
	public static PurePicker<Result<IMonitor>> GetAdjacentMonitor(
		HMONITOR handle = default,
		bool reverse = true,
		bool getFirst = false
	) =>
		(rootSector) =>
		{
			handle = handle.OrActiveMonitor(rootSector);
			ImmutableArray<IMonitor> monitors = rootSector.MonitorSector.Monitors;

			int monitorIdx = -1;
			for (int idx = 0; idx < monitors.Length; idx++)
			{
				if (handle == monitors[idx].Handle)
				{
					monitorIdx = idx;
					break;
				}
			}

			if (monitorIdx == -1)
			{
				if (getFirst)
				{
					return Result.FromValue(monitors[0]);
				}

				return Result.FromException<IMonitor>(StoreExceptions.MonitorNotFound(handle));
			}

			int delta = reverse ? -1 : 1;
			return Result.FromValue(monitors[(monitorIdx + delta).Mod(monitors.Length)]);
		};

	/// <summary>
	/// Get the monitor at the point <paramref name="point"/>
	/// </summary>
	/// <param name="point">
	/// The point to get the monitor for.
	/// </param>
	/// <param name="getFirst">
	/// When <see langword="true"/>, then returns the first monitor. Otherwise returns an exception in the
	/// result.
	/// </param>
	/// <returns></returns>
	public static Picker<Result<IMonitor>> GetMonitorAtPoint(IPoint<int> point, bool getFirst = false) =>
		new GetMonitorAtPointPicker(point, getFirst);
}

internal record GetMonitorAtPointPicker(IPoint<int> Point, bool GetFirst = false) : Picker<Result<IMonitor>>
{
	internal override Result<IMonitor> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		IMonitorSector sector = rootSector.MonitorSector;

		HMONITOR hmonitor = internalCtx.CoreNativeManager.MonitorFromPoint(
			Point.ToSystemPoint(),
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		for (int idx = 0; idx < sector.Monitors.Length; idx += 1)
		{
			IMonitor m = sector.Monitors[idx];
			if (m.Handle == hmonitor)
			{
				return Result.FromValue(m);
			}
		}

		if (GetFirst)
		{
			return Result.FromValue(sector.Monitors[0]);
		}

		return Result.FromException<IMonitor>(StoreExceptions.NoMonitorFoundAtPoint(Point));
	}
}
