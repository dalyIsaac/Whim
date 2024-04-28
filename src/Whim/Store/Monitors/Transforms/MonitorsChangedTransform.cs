using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DotNext;

namespace Whim;

/// <summary>
/// Transform for when the monitors have changed.
/// </summary>
internal record MonitorsChangedTransform : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Logger.Debug($"Monitors changed");
		MonitorSector sector = mutableRootSector.Monitors;

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
			UpdateMapSector(ctx, mutableRootSector, addedMonitors, removedMonitors);
		}

		sector.QueueEvent(args);

		return Empty.Result;
	}

	private static void UpdateMapSector(
		IContext ctx,
		MutableRootSector mutableRootSector,
		List<IMonitor> addedMonitors,
		List<IMonitor> removedMonitors
	)
	{
		MapSector mapSector = mutableRootSector.Maps;
		MonitorSector monitorSector = mutableRootSector.Monitors;

		monitorSector.MonitorsChangingTasks++;

		// Deactivate all workspaces.
		foreach (IWorkspace visibleWorkspace in ctx.Store.Pick(MapPickers.GetAllActiveWorkspaces))
		{
			visibleWorkspace.Deactivate();
		}

		// If a monitor was removed, remove the workspace from the map.
		foreach (IMonitor monitor in removedMonitors)
		{
			mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.Remove(monitor);

			if (!ctx.Store.Pick(MapPickers.GetWorkspaceForMonitor(monitor)).TryGet(out IWorkspace workspace))
			{
				Logger.Error($"Could not find workspace for monitor {monitor}");
				continue;
			}

			workspace.Deactivate();
		}

		// If a monitor was added, set it to an inactive workspace.
		foreach (IMonitor monitor in addedMonitors)
		{
			// Try find a workspace which doesn't have a monitor.
			IWorkspace? workspace = null;
			foreach (IWorkspace w in ctx.WorkspaceManager)
			{
				if (!ctx.Store.Pick(MapPickers.GetMonitorForWorkspace(w)).IsSuccessful)
				{
					workspace = w;
					mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(monitor, w);
					break;
				}
			}

			// If there's no workspace, create one.
			if (workspace is null)
			{
				if (ctx.WorkspaceManager.Add() is IWorkspace newWorkspace)
				{
					mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(monitor, newWorkspace);
				}
				else
				{
					continue;
				}
			}
		}

		// Hack to only accept window events after Windows has been given a chance to stop moving
		// windows around after a monitor change.
		// NOTE: ButlerEventHandlersTests has a test for this which only runs locally - it is
		// turned off in CI as it has proved flaky when running on GitHub Actions.
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
