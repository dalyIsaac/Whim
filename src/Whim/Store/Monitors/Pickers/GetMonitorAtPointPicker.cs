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
public record GetMonitorIndexAtPointPicker(IPoint<int> Point, bool GetFirst = false) : Picker<int?>
{
	internal override int? Execute(IContext ctx, IInternalContext internalCtx)
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
public record GetMonitorAtPointPicker(IPoint<int> Point, bool GetFirst = false) : Picker<IMonitor?>
{
	internal override IMonitor? Execute(IContext ctx, IInternalContext internalCtx)
	{
		int? idx = ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point, GetFirst));
		if (idx is not int idxVal)
		{
			return null;
		}

		MonitorSlice slice = ctx.Store.MonitorSlice;
		if (idxVal > slice.Monitors.Length)
		{
			Logger.Error("Monitor index is invalid");
			return null;
		}

		return slice.Monitors[idxVal];
	}
}
