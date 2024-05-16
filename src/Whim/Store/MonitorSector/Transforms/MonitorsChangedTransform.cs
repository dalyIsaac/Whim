using System.Collections.Generic;
using System.Collections.Immutable;
using DotNext;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Transform for when the monitors have changed.
/// </summary>
internal record MonitorsChangedTransform : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Logger.Debug($"Monitors changed");
		MonitorSector sector = mutableRootSector.MonitorSector;

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
				sector.PrimaryMonitorHandle = monitor.Handle;
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

		// Make sure the other monitor handles are set if they're unset.
		if (sector.ActiveMonitorHandle == (HMONITOR)0)
		{
			sector.ActiveMonitorHandle = sector.PrimaryMonitorHandle;
			sector.LastWhimActiveMonitorHandle = sector.PrimaryMonitorHandle;
		}

		sector.QueueEvent(args);

		return Unit.Result;
	}
}
