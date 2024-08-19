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
	/// If the monitor is not found, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceByMonitor(HMONITOR monitorHandle) =>
		rootSector =>
			rootSector.MapSector.MonitorWorkspaceMap.TryGetValue(monitorHandle, out WorkspaceId workspaceId)
				? PickWorkspaceById(workspaceId)(rootSector)
				: Result.FromException<IWorkspace>(StoreExceptions.MonitorNotFound(monitorHandle));

	/// <summary>
	/// Retrieves the workspace for the given window.
	/// </summary>
	/// <param name="windowHandle"></param>
	/// <returns>
	/// The workspace for the window, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the window is not tracked or does not belong to any workspace, then <see cref="Result{T, TError}.Error"/> will be returned.
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

			return Result.FromException<IWorkspace>(StoreExceptions.WindowNotFound(windowHandle));
		};

	/// <summary>
	/// Retrieves the monitor for the given workspace.
	/// </summary>
	/// <param name="searchWorkspaceId">
	/// The ID of the workspace to get the monitor for.
	/// </param>
	/// <returns>
	/// The monitor for the workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found or does not appear on any monitor, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IMonitor>> PickMonitorByWorkspace(WorkspaceId searchWorkspaceId) =>
		rootSector =>
		{
			HMONITOR monitorHandle = rootSector.MapSector.GetMonitorByWorkspace(searchWorkspaceId);

			return monitorHandle == default
				? Result.FromException<IMonitor>(StoreExceptions.NoMonitorFoundForWorkspace(searchWorkspaceId))
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
	/// If the window is not tracked or does not appear on any monitor, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IMonitor>> PickMonitorByWindow(HWND windowHandle) =>
		rootSector =>
			rootSector.MapSector.WindowWorkspaceMap.TryGetValue(windowHandle, out WorkspaceId workspaceId)
				? PickMonitorByWorkspace(workspaceId)(rootSector)
				: Result.FromException<IMonitor>(StoreExceptions.NoMonitorFoundForWindow(windowHandle));

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
	/// If the workspace is not found or there are no adjacent workspaces, then <see cref="Result{T, TError}.Error"/> will be returned.
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
				return Result.FromException<IWorkspace>(StoreExceptions.WorkspaceNotFound(workspaceId));
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

			return Result.FromException<IWorkspace>(new WhimException($"No adjacent workspace found to {workspaceId}"));
		};
}
