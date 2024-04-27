using DotNext;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Get the index for the monitor at the point <see cref="Point" />.
/// </summary>
/// <param name="Point">
/// The point to get the monitor for.
/// </param>
/// <param name="GetFirst">
/// When <see langword="true"/>, then returns the first monitor. Otherwise returns an exception in the
/// result.
/// </param>
public record GetMonitorIndexAtPointPicker(IPoint<int> Point, bool GetFirst = false) : Picker<Result<int>>
{
	internal override Result<int> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		Logger.Debug($"Getting monitor at point {Point}");
		IMonitorSector sector = rootSector.Monitors;

		HMONITOR hmonitor = internalCtx.CoreNativeManager.MonitorFromPoint(
			Point.ToSystemPoint(),
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		for (int idx = 0; idx < sector.Monitors.Length; idx += 1)
		{
			IMonitor m = sector.Monitors[idx];
			if (m.Handle == hmonitor)
			{
				return Result.FromValue(idx);
			}
		}

		if (GetFirst)
		{
			return Result.FromValue(0);
		}

		return Result.FromException<int>(new WhimException($"No monitor found at point {Point}"));
	}
}

/// <summary>
/// Get the monitor at the point <see cref="Point" />.
/// </summary>
/// <param name="Point">
/// The point to get the monitor for.
/// </param>
/// <param name="GetFirst">
/// When <see langword="true"/>, then returns the first monitor. Otherwise returns an exception in the
/// result.
/// </param>
public record GetMonitorAtPointPicker(IPoint<int> Point, bool GetFirst = false) : Picker<Result<IMonitor>>
{
	internal override Result<IMonitor> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		Result<int> idxResult = ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point, GetFirst));
		if (!idxResult.TryGet(out int idxVal))
		{
			return Result.FromException<IMonitor>(idxResult.Error!);
		}

		IMonitorSector sector = rootSector.Monitors;
		if (idxVal > sector.Monitors.Length)
		{
			return Result.FromException<IMonitor>(new WhimException("Monitor index is invalid"));
		}

		return Result.FromValue(sector.Monitors[idxVal]);
	}
}
