using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// Transform for when the monitors have changed.
/// </summary>
internal record MonitorsChangedTransform : Transform
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx, RootSlice root)
	{
		Logger.Debug($"Monitors changed");

		// Get the new monitors.
		ImmutableArray<IMonitor> previousMonitors = root.MonitorSlice.Monitors;

		root.MonitorSlice.Monitors = MonitorUtils.GetCurrentMonitors(internalCtx);

		List<IMonitor> unchangedMonitors = new();
		List<IMonitor> removedMonitors = new();
		List<IMonitor> addedMonitors = new();

		// For each monitor in the previous set, check if it's in the current set.
		foreach (IMonitor monitor in previousMonitors)
		{
			if (root.MonitorSlice.Monitors.Contains(monitor))
			{
				unchangedMonitors.Add(monitor);
			}
			else
			{
				removedMonitors.Add(monitor);
			}
		}

		// For each monitor in the current set, check if it's in the previous set.
		for (int idx = 0; idx < root.MonitorSlice.Monitors.Length; idx += 1)
		{
			IMonitor monitor = root.MonitorSlice.Monitors[idx];
			if (!previousMonitors.Contains(monitor))
			{
				addedMonitors.Add(monitor);
			}

			if (monitor.IsPrimary)
			{
				root.MonitorSlice.PrimaryMonitorIndex = idx;
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

		root.MonitorSlice.QueueEvent(args);
		// TODO: Emit event in MonitorManager.
	}
}
