using System.Threading.Tasks;
using DotNext;

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
			UpdateMapSector(ctx, mutableRootSector, addedMonitors, removedMonitors);
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

	private static void UpdateMapSector(
		IContext ctx,
		MutableRootSector rootSector,
		List<IMonitor> addedMonitors,
		List<IMonitor> removedMonitors
	)
	{
		MapSector mapSector = rootSector.MapSector;
		MonitorSector monitorSector = rootSector.MonitorSector;
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;

		if (!workspaceSector.HasInitialized)
		{
			return;
		}

		monitorSector.MonitorsChangingTasks++;

		// Deactivate all workspaces.
		foreach (IWorkspace visibleWorkspace in ctx.Store.Pick(PickAllActiveWorkspaces()))
		{
			visibleWorkspace.Deactivate();
		}

		// If a monitor was removed, remove the workspace from the map.
		foreach (IMonitor monitor in removedMonitors)
		{
			if (!ctx.Store.Pick(PickWorkspaceByMonitor(monitor.Handle)).TryGet(out IWorkspace workspace))
			{
				continue;
			}

			workspace.Deactivate();
			mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.Remove(monitor.Handle);
		}

		// If a monitor was added, set it to an inactive workspace.
		foreach (IMonitor monitor in addedMonitors)
		{
			// Try find a workspace which doesn't have a monitor.
			WorkspaceId workspaceId = default;
			foreach (WorkspaceId currId in workspaceSector.WorkspaceOrder)
			{
				if (!ctx.Store.Pick(PickMonitorByWorkspace(currId)).IsSuccessful)
				{
					workspaceId = currId;
					mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(monitor.Handle, currId);
					break;
				}
			}

			// If there's no workspace, create one.
			if (workspaceId == default)
			{
				if (ctx.WorkspaceManager.Add() is IWorkspace newWorkspace)
				{
					mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(
						monitor.Handle,
						newWorkspace.Id
					);
				}
				else
				{
					continue;
				}
			}
		}

		// Hack to only accept window events after Windows has been given a chance to stop moving
		// windows around after a monitor change.
		ctx.NativeManager.TryEnqueue(async () =>
		{
			await Task.Delay(monitorSector.MonitorsChangedDelay).ConfigureAwait(true);

			monitorSector.MonitorsChangingTasks--;
			if (monitorSector.MonitorsChangingTasks > 0)
			{
				Logger.Debug("Monitors changed: More tasks are pending");
				return;
			}

			Logger.Debug("Cleared AreMonitorsChanging");

			// For each workspace which is active in a monitor, do a layout.
			// This will handle cases when the monitor's properties have changed.
			ctx.Store.Dispatch(new LayoutAllActiveWorkspacesTransform());
		});
	}
}
