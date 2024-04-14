using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// Gets the monitor before the given monitor.
/// </summary>
/// <param name="Monitor"></param>
public record GetPreviousMonitorSelector(IMonitor Monitor) : Selector<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx)
	{
		ImmutableArray<IMonitor> monitors = ctx.Store.MonitorSlice.Monitors;

		int idx = monitors.IndexOf(Monitor);
		if (idx == -1)
		{
			Logger.Error($"Monitor {Monitor} not found.");
			return monitors[0];
		}

		return monitors[(idx - 1).Mod(monitors.Length)];
	}
}
