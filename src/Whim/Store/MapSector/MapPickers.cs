using System.Linq;

namespace Whim;

/// <summary>
/// Pure pickers for <see cref="IMapSector"/>.
/// </summary>
public static partial class Pickers
{
	/// <summary>
	/// Gets all the workspaces which are active on any monitor.
	/// </summary>
	/// <returns>
	/// All the active workspaces, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<IEnumerable<IWorkspace>> PickAllActiveWorkspaces() =>
		rootSector =>
		{
			List<IWorkspace> workspaces = [];
			foreach (WorkspaceId id in rootSector.MapSector.MonitorWorkspaceMap.Values)
			{
				workspaces.Add(rootSector.WorkspaceSector.Workspaces[id]);
			}
			return workspaces;
		};

	/// <summary>
	/// Retrieves the workspace shown on the given monitor.
	/// </summary>
	/// <param name="monitorHandle">
	/// The handle of the monitor to get the workspace for.
	/// </param>
	/// <returns>
	/// The workspace shown on the monitor, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the monitor is not found, then an error will be returned.
	/// </returns>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceByMonitor(HMONITOR monitorHandle) =>
		rootSector =>
			rootSector.MapSector.MonitorWorkspaceMap.TryGetValue(monitorHandle, out WorkspaceId workspaceId)
				? PickWorkspaceById(workspaceId)(rootSector)
				: Result.FromError<IWorkspace>(StoreErrors.MonitorNotFound(monitorHandle));

	/// <summary>
	/// Retrieves the workspace for the given window.
	/// </summary>
	/// <param name="windowHandle"></param>
	/// <returns>
	/// The workspace for the window, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the window is not tracked or does not belong to any workspace, then an error will be returned.
	/// </returns>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceByWindow(HWND windowHandle) =>
		rootSector =>
		{
			foreach ((HWND currentHandle, WorkspaceId workspaceId) in rootSector.MapSector.WindowWorkspaceMap)
			{
				if (currentHandle == windowHandle)
				{
					return PickWorkspaceById(workspaceId)(rootSector);
				}
			}

			return Result.FromError<IWorkspace>(StoreErrors.WindowNotFound(windowHandle));
		};

	/// <summary>
	/// Retrieves the monitor for the given workspace.
	/// </summary>
	/// <param name="searchWorkspaceId">
	/// The ID of the workspace to get the monitor for.
	/// </param>
	/// <returns>
	/// The monitor for the workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found or does not appear on any monitor, then an error will be returned.
	/// </returns>
	public static PurePicker<Result<IMonitor>> PickMonitorByWorkspace(WorkspaceId searchWorkspaceId) =>
		rootSector =>
		{
			HMONITOR monitorHandle = rootSector.MapSector.GetMonitorByWorkspace(searchWorkspaceId);

			return monitorHandle == default
				? Result.FromError<IMonitor>(StoreErrors.NoMonitorFoundForWorkspace(searchWorkspaceId))
				: PickMonitorByHandle(monitorHandle)(rootSector);
		};

	/// <summary>
	/// Retrieves the monitor for the given window.
	/// </summary>
	/// <param name="windowHandle">
	/// The handle of the window to get the monitor for.
	/// </param>
	/// <returns>
	/// The monitor for the window, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the window is not tracked or does not appear on any monitor, then an error will be returned.
	/// </returns>
	public static PurePicker<Result<IMonitor>> PickMonitorByWindow(HWND windowHandle) =>
		rootSector =>
			rootSector.MapSector.WindowWorkspaceMap.TryGetValue(windowHandle, out WorkspaceId workspaceId)
				? PickMonitorByWorkspace(workspaceId)(rootSector)
				: Result.FromError<IMonitor>(StoreErrors.NoMonitorFoundForWindow(windowHandle));

	/// <summary>
	/// Gets the adjacent workspace for the given workspace.
	/// </summary>
	/// <param name="workspaceId">
	/// The workspace to get the adjacent workspace for.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, gets the previous workspace, otherwise gets the next workspace. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	/// <returns>
	/// The adjacent workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found or there are no adjacent workspaces, then an error will be returned.
	/// </returns>
	public static PurePicker<Result<IWorkspace>> PickAdjacentWorkspace(
		WorkspaceId workspaceId,
		bool reverse = false,
		bool skipActive = false
	) =>
		rootSector =>
		{
			IWorkspaceSector workspaceSector = rootSector.WorkspaceSector;
			IMapSector mapSector = rootSector.MapSector;

			ImmutableArray<WorkspaceId> order = workspaceSector.WorkspaceOrder;
			int idx = order.IndexOf(workspaceId);

			if (idx == -1)
			{
				return Result.FromError<IWorkspace>(StoreErrors.WorkspaceNotFound(workspaceId));
			}

			WorkspaceId activeWorkspaceId = PickActiveWorkspaceId()(rootSector);

			int delta = reverse ? -1 : 1;
			int nextIdx = (idx + delta).Mod(order.Length);
			while (idx != nextIdx)
			{
				WorkspaceId nextWorkspaceId = order[nextIdx];

				bool isActive = mapSector.MonitorWorkspaceMap.ContainsValue(nextWorkspaceId);

				if (!skipActive || !isActive)
				{
					return workspaceSector.Workspaces[nextWorkspaceId];
				}

				nextIdx = (nextIdx + delta).Mod(order.Length);
			}

			return Result.FromError<IWorkspace>(new WhimError($"No adjacent workspace found to {workspaceId}"));
		};

	/// <summary>
	/// Retrieves the active layout engine for the workspace on the given monitor.
	/// </summary>
	/// <param name="monitorHandle">
	/// The handle of the monitor to get the active layout engine for.
	/// </param>
	/// <returns>
	/// The active layout engine for the workspace on the monitor, otherwise an error.
	/// </returns>
	public static PurePicker<Result<ILayoutEngine>> PickActiveLayoutEngineByMonitor(HMONITOR monitorHandle) =>
		rootSector =>
		{
			Result<IWorkspace> workspaceResult = PickWorkspaceByMonitor(monitorHandle)(rootSector);

			if (!workspaceResult.TryGet(out IWorkspace workspace))
			{
				return Result.FromError<ILayoutEngine>(workspaceResult.Error!);
			}

			return Result.FromValue(WorkspaceUtils.GetActiveLayoutEngine(workspace));
		};

	/// <summary>
	/// Retrieves the handles of the monitors which can show the given workspace. This includes workspaces
	/// which explicitly state which monitors they can be shown on, and workspaces which can be shown on any monitor
	/// (i.e., they don't specify any monitors).
	/// </summary>
	/// <param name="workspaceId">
	/// The ID of the workspace to get the monitors for.
	/// </param>
	/// <returns>
	/// The handles of the monitors which can show the workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If there are no explicit monitors which can show the workspace, then all monitors will be returned.
	/// If the workspace is not found, then an error will be returned.
	/// </returns>
	public static PurePicker<Result<IReadOnlyList<HMONITOR>>> PickStickyMonitorsByWorkspace(WorkspaceId workspaceId) =>
		rootSector =>
		{
			IMapSector mapSector = rootSector.MapSector;
			IWorkspaceSector workspaceSector = rootSector.WorkspaceSector;
			ImmutableArray<IMonitor> monitors = rootSector.MonitorSector.Monitors;

			if (!workspaceSector.Workspaces.ContainsKey(workspaceId))
			{
				return Result.FromError<IReadOnlyList<HMONITOR>>(StoreErrors.WorkspaceNotFound(workspaceId));
			}

			// If the workspace is sticky, try get the associated monitors.
			if (
				mapSector.StickyWorkspaceMonitorIndexMap.TryGetValue(
					workspaceId,
					out ImmutableArray<int> monitorIndices
				)
			)
			{
				List<HMONITOR> monitorHandles =
				[
					.. monitorIndices
						.Where(monitorIndex => monitorIndex >= 0 && monitorIndex < monitors.Length)
						.Select(monitorIndex => monitors[monitorIndex].Handle),
				];

				if (monitorHandles.Count != 0)
				{
					return monitorHandles;
				}
			}

			// If the workspace isn't sticky, or there are no longer any valid monitors for it, allow it on all monitors.
			return monitors.Select(monitor => monitor.Handle).ToList();
		};

	/// <summary>
	/// Retrieves the explicit indices of the monitors which can show the given workspace.
	/// </summary>
	/// <param name="workspaceId">
	/// The ID of the workspace to get the monitor indices for.
	/// </param>
	/// <returns>
	/// The explicit indices of the monitors which can show the workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace cannot be found, or if the workspace has no explicit mappings, then an error is returned.
	/// </returns>
	public static PurePicker<Result<IReadOnlyList<int>>> PickExplicitStickyMonitorIndicesByWorkspace(
		WorkspaceId workspaceId
	) =>
		rootSector =>
		{
			IMapSector mapSector = rootSector.MapSector;
			IWorkspaceSector workspaceSector = rootSector.WorkspaceSector;

			if (
				mapSector.StickyWorkspaceMonitorIndexMap.TryGetValue(
					workspaceId,
					out ImmutableArray<int> monitorIndices
				)
			)
			{
				return monitorIndices;
			}

			return Result.FromError<IReadOnlyList<int>>(
				new WhimError($"No explicit monitor indices found for {workspaceId}")
			);
		};

	/// <summary>
	/// Retrieves the first suitable monitor for a workspace, if one is not provided. A monitor is suitable for a workspace if:
	///
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// If the workspace lists the monitor's index as one of its sticky monitors
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// If the workspace does not list any sticky monitors
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// If the workspace lists all non-existent monitors as sticky monitors
	/// </description>
	/// </item>
	/// </list>
	///
	/// The order of suitability is:
	///
	/// <list type="number">
	/// <item>
	/// <description>
	/// The provided <paramref name="monitorHandle"/>
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// The last monitor the workspace was activated on
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// The first available monitor
	/// </description>
	/// </item>
	/// </list>
	/// </summary>
	/// <param name="workspaceId">
	/// The ID of the workspace to get the monitor for.
	/// </param>
	/// <param name="monitorHandle">
	/// The preferred monitor to use. If not provided, the last monitor the workspace was activated on will next be tried.
	/// </param>
	/// <returns>
	/// The first valid monitor for the workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace can't be found, then an error is returned.
	/// </returns>
	public static PurePicker<Result<HMONITOR>> PickValidMonitorByWorkspace(
		WorkspaceId workspaceId,
		HMONITOR monitorHandle = default
	) =>
		rootSector =>
		{
			// Verify the workspace exists.
			if (!rootSector.WorkspaceSector.Workspaces.ContainsKey(workspaceId))
			{
				return Result.FromError<HMONITOR>(StoreErrors.WorkspaceNotFound(workspaceId));
			}

			// Get the valid monitors for the workspace.
			IReadOnlyList<HMONITOR> validMonitors =
				PickStickyMonitorsByWorkspace(workspaceId)(rootSector).ValueOrDefault ?? [];

			// Try activate on the current monitor.
			HMONITOR targetMonitorHandle = monitorHandle;
			if (targetMonitorHandle == default)
			{
				targetMonitorHandle = rootSector.MonitorSector.ActiveMonitorHandle;
			}

			if (validMonitors.Contains(targetMonitorHandle))
			{
				return targetMonitorHandle;
			}

			Logger.Debug(
				$"Monitor {targetMonitorHandle} is not valid for workspace {workspaceId}, falling back to the last monitor the workspace was activated on"
			);

			// If the monitor is not valid, try activate on the last monitor.
			if (rootSector.MapSector.WorkspaceLastMonitorMap.TryGetValue(workspaceId, out HMONITOR lastMonitorHandle))
			{
				if (validMonitors.Contains(lastMonitorHandle))
				{
					return lastMonitorHandle;
				}
			}

			Logger.Debug(
				$"Monitor {lastMonitorHandle} is not valid for workspace {workspaceId}, falling back to first monitor available"
			);

			// Activate on the first available monitor.
			foreach (IMonitor monitor in rootSector.MonitorSector.Monitors)
			{
				if (validMonitors.Contains(monitor.Handle))
				{
					return monitor.Handle;
				}
			}

			return Result.FromError<HMONITOR>(StoreErrors.NoValidMonitorForWorkspace(workspaceId));
		};

	/// <summary>
	/// Retrieves the workspaces which can be shown on the given monitor.
	/// </summary>
	/// <param name="monitorHandle">
	/// The handle of the monitor to get the workspaces for.
	/// </param>
	/// <returns>
	/// The workspaces which can be shown on the monitor, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the monitor can't be found, then an error is returned.
	/// </returns>
	public static PurePicker<Result<IReadOnlyList<IWorkspace>>> PickStickyWorkspacesByMonitor(HMONITOR monitorHandle) =>
		rootSector =>
		{
			IMapSector mapSector = rootSector.MapSector;
			IWorkspaceSector workspaceSector = rootSector.WorkspaceSector;
			IMonitorSector monitorSector = rootSector.MonitorSector;

			// Verify the monitor exists.
			Result<IMonitor> monitorResult = PickMonitorByHandle(monitorHandle)(rootSector);
			if (!monitorResult.TryGet(out IMonitor monitor))
			{
				return Result.FromError<IReadOnlyList<IWorkspace>>(monitorResult.Error!);
			}

			// Get the index of the monitor.
			ImmutableArray<IMonitor> monitors = monitorSector.Monitors;
			int monitorIndex = monitors.IndexOf(monitor);

			// Get the workspaces which can be shown on the monitor.
			List<WorkspaceId> processedWorkspaces = [];
			List<WorkspaceId> unsortedWorkspaces = [];

			foreach (
				(
					WorkspaceId workspaceId,
					ImmutableArray<int> monitorIndices
				) in mapSector.StickyWorkspaceMonitorIndexMap
			)
			{
				// If the workspace is sticky to the monitor, or it's orphaned.
				if (
					monitorIndices.Contains(monitorIndex)
					|| monitorIndices.All(index => index < 0 || index >= monitors.Length)
				)
				{
					unsortedWorkspaces.Add(workspaceId);
				}

				processedWorkspaces.Add(workspaceId);
			}

			// Get the workspaces which can be shown on any monitor.
			foreach (WorkspaceId workspaceId in workspaceSector.Workspaces.Keys)
			{
				if (processedWorkspaces.Contains(workspaceId))
				{
					continue;
				}

				unsortedWorkspaces.Add(workspaceId);
			}

			// Crude sorting.
			List<IWorkspace> sortedWorkspaces = [];
			foreach (WorkspaceId workspaceId in workspaceSector.WorkspaceOrder)
			{
				if (unsortedWorkspaces.Contains(workspaceId))
				{
					sortedWorkspaces.Add(workspaceSector.Workspaces[workspaceId]);
				}
			}

			return sortedWorkspaces;
		};
}
