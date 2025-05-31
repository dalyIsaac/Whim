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

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = ctx.Store.Pick(PickWorkspaceByMonitor(targetMonitor.Handle)).ValueOrDefault;

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = ctx.Store.Pick(PickMonitorByWorkspace(WorkspaceId)).ValueOrDefault;

		if (targetMonitor.Handle == loserMonitor?.Handle)
		{
			Logger.Debug("Workspace is already activated");
			return Unit.Result;
		}

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(targetMonitor.Handle, WorkspaceId);

		if (loserMonitor != null && oldWorkspace != null && loserMonitor.Handle != targetMonitor.Handle)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(loserMonitor.Handle, oldWorkspace.Id);

			ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(oldWorkspace.Id));
			mapSector.QueueEvent(
				new MonitorWorkspaceChangedEventArgs()
				{
					Monitor = loserMonitor,
					PreviousWorkspace = workspace,
					CurrentWorkspace = oldWorkspace,
				}
			);
		}
		else
		{
			if (oldWorkspace is not null)
			{
				ctx.Store.Dispatch(new DeactivateWorkspaceTransform(oldWorkspace.Id));
			}

			// Temporarily focus the monitor's desktop HWND, to prevent another window from being focused.
			ctx.Store.Dispatch(new FocusMonitorDesktopTransform(targetMonitor.Handle));
		}

		// Layout the new workspace.
		ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(workspace.Id));

		if (FocusWorkspaceWindow)
		{
			ctx.Store.Dispatch(new FocusWorkspaceTransform(workspace.Id));
		}
		else
		{
			WorkspaceId activeWorkspaceId = ctx.Store.Pick(PickActiveWorkspaceId());
			ctx.Store.Dispatch(new FocusWorkspaceTransform(activeWorkspaceId));
		}

		mapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = targetMonitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = workspace,
			}
		);

		return Unit.Result;
	}
}
