using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Gets the monitor before the given monitor.
/// </summary>
/// <param name="Monitor"></param>
public record GetPreviousMonitorPicker(IMonitor Monitor) : Picker<Result<IMonitor>>()
{
	internal override Result<IMonitor> Execute(IContext ctx, IInternalContext internalCtx)
	{
		ImmutableArray<IMonitor> monitors = ctx.Store.MonitorSlice.Monitors;

		int idx = monitors.IndexOf(Monitor);
		if (idx == -1)
		{
			return Result.FromException<IMonitor>(new WhimException($"Monitor {Monitor} not found."));
		}

		return Result.FromValue(monitors[(idx - 1).Mod(monitors.Length)]);
	}
}
