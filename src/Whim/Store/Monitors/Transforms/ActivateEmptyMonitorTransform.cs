namespace Whim;

/// <summary>
/// Activate the monitor even if it's empty.
/// </summary>
/// <param name="Monitor">
/// The monitor to activate.
/// </param>
public record ActivateEmptyMonitorTransform(IMonitor Monitor) : Transform
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx, RootSlice root)
	{
		if (!root.MonitorSlice.Monitors.Contains(Monitor))
		{
			Logger.Error($"Monitor {Monitor} not found.");
			return;
		}

		int idx = root.MonitorSlice.Monitors.IndexOf(Monitor);
		root.MonitorSlice.ActiveMonitorIndex = idx;
	}
}
