using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Gets the monitor after the given monitor.
/// </summary>
/// <param name="Monitor"></param>
/// <param name="GetFirst">
/// When <see langword="true"/>, then returns the first monitor. Otherwise returns an exception in the
/// result.
/// </param>
public record GetNextMonitorPicker(IMonitor Monitor, bool GetFirst = false) : Picker<Result<IMonitor>>()
{
	internal override Result<IMonitor> Execute(IContext ctx, IInternalContext internalCtx)
	{
		ImmutableArray<IMonitor> monitors = ctx.Store.Monitors.Monitors;

		int idx = monitors.IndexOf(Monitor);
		if (idx == -1)
		{
			if (GetFirst)
			{
				return Result.FromValue(monitors[0]);
			}

			return Result.FromException<IMonitor>(new WhimException($"Monitor {Monitor} not found."));
		}

		return Result.FromValue(monitors[(idx + 1).Mod(monitors.Length)]);
	}
}
