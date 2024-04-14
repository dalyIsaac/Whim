using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// Transform for when the monitors have changed.
/// </summary>
internal record MonitorsChangedTransform : Transform
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		Logger.Debug($"Monitors changed");

		// Get the new monitors.
		ImmutableArray<IMonitor> previousMonitors = ctx.Store.MonitorSlice.Monitors;

		ctx.Store.MonitorSlice.Monitors = MonitorUtils.GetCurrentMonitors(internalCtx);

		List<IMonitor> unchangedMonitors = new();
		List<IMonitor> removedMonitors = new();
		List<IMonitor> addedMonitors = new();

		// For each monitor in the previous set, check if it's in the current set.
		foreach (IMonitor monitor in previousMonitors)
		{
			if (ctx.Store.MonitorSlice.Monitors.Contains(monitor))
			{
				unchangedMonitors.Add(monitor);
			}
			else
			{
				removedMonitors.Add(monitor);
			}
		}

		// For each monitor in the current set, check if it's in the previous set.
		for (int idx = 0; idx < ctx.Store.MonitorSlice.Monitors.Length; idx += 1)
		{
			IMonitor monitor = ctx.Store.MonitorSlice.Monitors[idx];
			if (!previousMonitors.Contains(monitor))
			{
				addedMonitors.Add(monitor);
			}

			if (monitor.IsPrimary)
			{
				ctx.Store.MonitorSlice.PrimaryMonitorIndex = idx;
			}
		}

		MonitorsChangedEventArgs args =
			new()
			{
				UnchangedMonitors = unchangedMonitors,
				RemovedMonitors = removedMonitors,
				AddedMonitors = addedMonitors
			};

		if (addedMonitors.Count != 0 || removedMonitors.Count != 0)
		{
			internalCtx.ButlerEventHandlers.OnMonitorsChanged(args);
		}

		ctx.Store.MonitorSlice.QueueEvent(args);
		// TODO: Emit event in MonitorManager.
	}
}
