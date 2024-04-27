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
		MonitorSector sector = ctx.Store.Monitors;

		if (!sector.Monitors.Contains(Monitor))
		{
			Logger.Error($"Monitor {Monitor} not found.");
			return Empty.Result;
		}

		int idx = sector.Monitors.IndexOf(Monitor);
		sector.ActiveMonitorIndex = idx;
		return Empty.Result;
	}
}
