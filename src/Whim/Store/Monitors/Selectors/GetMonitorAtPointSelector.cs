using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Get the index for the monitor at the point <see cref="Point" />.
/// </summary>
/// <param name="Point">
/// The point to get the monitor for.
/// </param>
/// <param name="GetFirst">
/// If <see langword="true"/>, get the first monitor if there is no monitor at <paramref name="Point"/>.
/// </param>
public record GetMonitorIndexAtPointSelector(IPoint<int> Point, bool GetFirst = false) : Selector<int?>
{
	internal override int? Execute(IContext ctx, IInternalContext internalCtx)
	{
		Logger.Debug($"Getting monitor at point {Point}");
		HMONITOR hmonitor = internalCtx.CoreNativeManager.MonitorFromPoint(
			Point.ToSystemPoint(),
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		for (int idx = 0; idx < ctx.Store.MonitorSlice.Monitors.Length; idx += 1)
		{
			IMonitor m = ctx.Store.MonitorSlice.Monitors[idx];
			if (m.Handle == hmonitor)
			{
				return idx;
			}
		}

		if (GetFirst)
		{
			return 0;
		}

		Logger.Error($"No monitor found at point {Point}");
		return null;
	}
}

/// <summary>
/// Get the monitor at the point <see cref="Point" />.
/// </summary>
/// <param name="Point">
/// The point to get the monitor for.
/// </param>
/// <param name="GetFirst">
/// If <see langword="true"/>, get the first monitor if there is no monitor at <paramref name="Point"/>.
/// </param>
public record GetMonitorAtPointSelector(IPoint<int> Point, bool GetFirst = false) : Selector<IMonitor?>
{
	internal override IMonitor? Execute(IContext ctx, IInternalContext internalCtx)
	{
		int? idx = ctx.Store.Select(new GetMonitorIndexAtPointSelector(Point, GetFirst));
		if (idx is not int idxVal)
		{
			return null;
		}

		if (idxVal > ctx.Store.MonitorSlice.Monitors.Length)
		{
			Logger.Error("Monitor index is invalid");
			return null;
		}

		return ctx.Store.MonitorSlice.Monitors[idxVal];
	}
}
