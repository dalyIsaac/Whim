using DotNext;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Get the index for the monitor at the point <see cref="Point" />.
/// </summary>
/// <param name="Point">
/// The point to get the monitor for.
/// </param>
public record GetMonitorIndexAtPointPicker(IPoint<int> Point) : Picker<Result<int>>
{
	internal override Result<int> Execute(IContext ctx, IInternalContext internalCtx)
	{
		Logger.Debug($"Getting monitor at point {Point}");
		MonitorSlice slice = ctx.Store.MonitorSlice;

		HMONITOR hmonitor = internalCtx.CoreNativeManager.MonitorFromPoint(
			Point.ToSystemPoint(),
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		for (int idx = 0; idx < slice.Monitors.Length; idx += 1)
		{
			IMonitor m = slice.Monitors[idx];
			if (m.Handle == hmonitor)
			{
				return Result.FromValue(idx);
			}
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
public record GetMonitorAtPointPicker(IPoint<int> Point) : Picker<Result<IMonitor>>
{
	internal override Result<IMonitor> Execute(IContext ctx, IInternalContext internalCtx)
	{
		Result<int> idxResult = ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point));
		if (!idxResult.TryGet(out int idxVal))
		{
			return Result.FromException<IMonitor>(idxResult.Error!);
		}

		MonitorSlice slice = ctx.Store.MonitorSlice;
		if (idxVal > slice.Monitors.Length)
		{
			return Result.FromException<IMonitor>(new WhimException("Monitor index is invalid"));
		}

		return Result.FromValue(slice.Monitors[idxVal]);
	}
}
