using System.Collections.Generic;
using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Transform for when the monitors have changed.
/// </summary>
internal record MonitorsChangedTransform : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		Logger.Debug($"Monitors changed");
		MonitorSector sector = ctx.Store.Monitors;

		// Get the new monitors.
		ImmutableArray<IMonitor> previousMonitors = sector.Monitors;

		sector.Monitors = MonitorUtils.GetCurrentMonitors(internalCtx);

		List<IMonitor> unchangedMonitors = new();
		List<IMonitor> removedMonitors = new();
		List<IMonitor> addedMonitors = new();

		// For each monitor in the previous set, check if it's in the current set.
		foreach (IMonitor monitor in previousMonitors)
		{
			if (sector.Monitors.Contains(monitor))
			{
				unchangedMonitors.Add(monitor);
			}
			else
			{
				removedMonitors.Add(monitor);
			}
		}

		// For each monitor in the current set, check if it's in the previous set.
		for (int idx = 0; idx < sector.Monitors.Length; idx += 1)
		{
			IMonitor monitor = sector.Monitors[idx];
			if (!previousMonitors.Contains(monitor))
			{
				addedMonitors.Add(monitor);
			}

			if (monitor.IsPrimary)
			{
				sector.PrimaryMonitorIndex = idx;
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

		sector.QueueEvent(args);

		return Empty.Result;
	}
}
