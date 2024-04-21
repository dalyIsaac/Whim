using DotNext;

namespace Whim;

/// <summary>
/// Activate the monitor even if it's empty.
/// </summary>
/// <param name="Monitor">
/// The monitor to activate.
/// </param>
public record ActivateEmptyMonitorTransform(IMonitor Monitor) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		MonitorSlice slice = ctx.Store.MonitorSlice;

		if (!slice.Monitors.Contains(Monitor))
		{
			Logger.Error($"Monitor {Monitor} not found.");
			return Empty.Result;
		}

		int idx = slice.Monitors.IndexOf(Monitor);
		slice.ActiveMonitorIndex = idx;
		return Empty.Result;
	}
}
