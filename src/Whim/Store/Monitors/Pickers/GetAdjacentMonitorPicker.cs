using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Gets the monitor before the given monitor.
/// </summary>
/// <param name="Monitor"></param>
/// <param name="Reverse">
/// When <see langword="true"/>, gets the previous monitor, otherwise gets the next monitor. Defaults to <see langword="true" />.
/// </param>
/// <param name="GetFirst">
/// When <see langword="true"/>, then returns the first monitor. Otherwise returns an exception in the
/// result.
/// </param>
public record GetAdjacentMonitorPicker(IMonitor Monitor, bool Reverse = true, bool GetFirst = false)
	: Picker<Result<IMonitor>>()
{
	internal override Result<IMonitor> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		ImmutableArray<IMonitor> monitors = rootSector.Monitors.Monitors;

		int idx = monitors.IndexOf(Monitor);
		if (idx == -1)
		{
			if (GetFirst)
			{
				return Result.FromValue(monitors[0]);
			}

			return Result.FromException<IMonitor>(StoreExceptions.MonitorNotFound(Monitor));
		}

		int delta = Reverse ? -1 : 1;
		return Result.FromValue(monitors[(idx + delta).Mod(monitors.Length)]);
	}
}
