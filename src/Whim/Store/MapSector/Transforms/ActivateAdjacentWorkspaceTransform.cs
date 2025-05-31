namespace Whim;

/// <summary>
/// Activates the next (or previous) workspace in the given monitor.
/// </summary>
/// <param name="MonitorHandle">
/// The monitor to activate the next workspace in. Defaults to <see cref="IMonitorManager.ActiveMonitor"/>.
/// </param>
/// <param name="Reverse">
/// When <see langword="true"/>, gets the previous monitor, otherwise gets the next monitor. Defaults to <see langword="false" />.
/// </param>
/// <param name="SkipActive">
/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
/// </param>
public record ActivateAdjacentWorkspaceTransform(
	HMONITOR MonitorHandle = default,
	bool Reverse = false,
	bool SkipActive = false
) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector mapSector = rootSector.MapSector;
		HMONITOR targetMonitorHandle = MonitorHandle.OrActiveMonitor(rootSector);

		if (!mapSector.MonitorWorkspaceMap.TryGetValue(targetMonitorHandle, out WorkspaceId currentWorkspaceId))
		{
			return new(StoreErrors.NoWorkspaceFoundForMonitor(targetMonitorHandle));
		}

		Result<IWorkspace> nextWorkspaceResult = ctx.Store.Pick(
			PickAdjacentWorkspace(currentWorkspaceId, Reverse, SkipActive)
		);
		if (!nextWorkspaceResult.TryGet(out IWorkspace? nextWorkspace))
		{
			return Result.FromError<Unit>(nextWorkspaceResult.Error!);
		}

		return ctx.Store.Dispatch(new ActivateWorkspaceTransform(nextWorkspace.Id, targetMonitorHandle));
	}
}
