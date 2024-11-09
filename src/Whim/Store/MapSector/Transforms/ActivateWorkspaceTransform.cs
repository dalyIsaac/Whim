using System.Linq;

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
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		Result<HMONITOR> targetMonitorHandleResult = GetValidMonitorForWorkspace(ctx, rootSector, workspace);
		if (!targetMonitorHandleResult.TryGet(out HMONITOR targetMonitorHandle))
		{
			return Result.FromException<Unit>(targetMonitorHandleResult.Error!);
		}
		mapSector.WorkspaceLastMonitorMap = mapSector.WorkspaceLastMonitorMap.SetItem(
			workspace.Id,
			targetMonitorHandle
		);

		Result<IMonitor> targetMonitorResult = ctx.Store.Pick(PickMonitorByHandle(targetMonitorHandle));
		if (!targetMonitorResult.TryGet(out IMonitor targetMonitor))
		{
			return Result.FromException<Unit>(targetMonitorHandleResult.Error!);
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

	/// <summary>
	/// Gets the monitor to activate the workspace on, if one is not provided.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="rootSector"></param>
	/// <param name="workspace"></param>
	/// <returns></returns>
	private Result<HMONITOR> GetValidMonitorForWorkspace(
		IContext ctx,
		MutableRootSector rootSector,
		IWorkspace workspace
	)
	{
		// Get the valid monitors for the workspace.
		IReadOnlyList<HMONITOR> validMonitors =
			ctx.Store.Pick(PickStickyMonitorsByWorkspace(workspace.Id)).ValueOrDefault ?? [];

		// Try activate on the current monitor.
		HMONITOR targetMonitorHandle = MonitorHandle;
		if (targetMonitorHandle == default)
		{
			targetMonitorHandle = rootSector.MonitorSector.ActiveMonitorHandle;
		}

		if (validMonitors.Contains(targetMonitorHandle))
		{
			return targetMonitorHandle;
		}

		Logger.Debug(
			$"Monitor {targetMonitorHandle} is not valid for workspace {workspace.Id}, falling back to the last monitor the workspace was activated on"
		);

		// If the monitor is not valid, try activate on the last monitor.
		if (rootSector.MapSector.WorkspaceLastMonitorMap.TryGetValue(workspace.Id, out HMONITOR lastMonitorHandle))
		{
			if (validMonitors.Contains(lastMonitorHandle))
			{
				return targetMonitorHandle;
			}
		}

		Logger.Debug(
			$"Monitor {lastMonitorHandle} is not valid for workspace {workspace.Id}, falling back to first monitor available"
		);

		// Activate on the first available monitor.
		foreach (IMonitor monitor in rootSector.MonitorSector.Monitors)
		{
			if (validMonitors.Contains(monitor.Handle))
			{
				return monitor.Handle;
			}
		}

		return Result.FromException<HMONITOR>(StoreExceptions.NoValidMonitorForWorkspace(workspace.Id));
	}
}
