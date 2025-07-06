namespace Whim;

/// <summary>
/// Activates the given workspace in the active monitor, or the given monitor (if provided).
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to activate.
/// </param>
/// <param name="MonitorHandle">
/// The handle of the monitor to activate the workspace in. If <see langword="null"/>, this will
/// default to the active monitor.
/// </param>
/// <param name="FocusWorkspaceWindow">
/// If <see langword="true"/>, the last focused window of the <see cref="WorkspaceId"/> will be focused.
/// If <see langword="false"/>, the last focused window of the active workspace will be focused.
/// </param>
public record ActivateWorkspaceTransform(
	WorkspaceId WorkspaceId,
	HMONITOR MonitorHandle = default,
	bool FocusWorkspaceWindow = true
) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector mapSector = rootSector.MapSector;

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceById(WorkspaceId));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromError<Unit>(workspaceResult.Error!);
		}

		Result<HMONITOR> targetMonitorHandleResult = ctx.Store.Pick(
			PickValidMonitorByWorkspace(workspace.Id, MonitorHandle)
		);
		if (!targetMonitorHandleResult.TryGet(out HMONITOR targetMonitorHandle))
		{
			return Result.FromError<Unit>(targetMonitorHandleResult.Error!);
		}

		Result<IMonitor> targetMonitorResult = ctx.Store.Pick(PickMonitorByHandle(targetMonitorHandle));
		if (!targetMonitorResult.TryGet(out IMonitor targetMonitor))
		{
			return Result.FromError<Unit>(targetMonitorHandleResult.Error!);
		}

		return ActivateWorkspaceOnTargetMonitor(ctx, mapSector, workspace, targetMonitor);
	}

	private Result<Unit> ActivateWorkspaceOnTargetMonitor(
		IContext ctx,
		MapSector mapSector,
		IWorkspace workspace,
		IMonitor targetMonitor
	)
	{
		mapSector.WorkspaceLastMonitorMap = mapSector.WorkspaceLastMonitorMap.SetItem(
			workspace.Id,
			targetMonitor.Handle
		);

		// We won't have an old workspace for the target monitor during startup.
		IWorkspace? oldWorkspaceForTarget = ctx.Store.Pick(PickWorkspaceByMonitor(targetMonitor.Handle)).ValueOrDefault;

		IMonitor? sourceMonitor = ctx.Store.Pick(PickMonitorByWorkspace(WorkspaceId)).ValueOrDefault;

		if (targetMonitor.Handle == sourceMonitor?.Handle)
		{
			Logger.Debug("Workspace is already activated");
			return Unit.Result;
		}

		// We need to update the monitor workspace map for the target monitor before performing a layout on the loser monitor.
		mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(targetMonitor.Handle, WorkspaceId);

		if (sourceMonitor != null && oldWorkspaceForTarget != null)
		{
			LayoutWorkspaceOnLoserMonitor(ctx, mapSector, sourceMonitor, oldWorkspaceForTarget, workspace);
		}
		else if (oldWorkspaceForTarget != null)
		{
			ctx.Store.Dispatch(new DeactivateWorkspaceTransform(oldWorkspaceForTarget.Id));
		}

		ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(workspace.Id));

		WorkspaceId workspaceToFocus = FocusWorkspaceWindow ? workspace.Id : ctx.Store.Pick(PickActiveWorkspaceId());
		ctx.Store.Dispatch(new FocusWorkspaceTransform(workspaceToFocus));

		mapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = targetMonitor,
				PreviousWorkspace = oldWorkspaceForTarget,
				CurrentWorkspace = workspace,
			}
		);

		return Unit.Result;
	}

	private static void LayoutWorkspaceOnLoserMonitor(
		IContext ctx,
		MapSector mapSector,
		IMonitor monitor,
		IWorkspace workspace,
		IWorkspace oldWorkspace
	)
	{
		Logger.Debug($"Performing layout for workspace {workspace} in loser monitor {monitor}");
		mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(monitor.Handle, workspace.Id);
		ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(workspace.Id));
		mapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = monitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = workspace,
			}
		);
	}
}
