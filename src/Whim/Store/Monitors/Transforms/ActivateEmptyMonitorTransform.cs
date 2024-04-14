namespace Whim;

/// <summary>
/// Activate the monitor even if it's empty.
/// </summary>
/// <param name="Monitor">
/// The monitor to activate.
/// </param>
public record ActivateEmptyMonitorTransform(IMonitor Monitor) : Transform
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		if (!ctx.Store.MonitorSlice.Monitors.Contains(Monitor))
		{
			Logger.Error($"Monitor {Monitor} not found.");
			return;
		}

		int idx = ctx.Store.MonitorSlice.Monitors.IndexOf(Monitor);
		ctx.Store.MonitorSlice.ActiveMonitorIndex = idx;
	}
}
