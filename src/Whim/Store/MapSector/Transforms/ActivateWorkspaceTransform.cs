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
public record ActivateWorkspaceTransform(WorkspaceId WorkspaceId, HMONITOR MonitorHandle = default) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Get the workspace.
		if (!rootSector.WorkspaceSector.Workspaces.TryGetValue(WorkspaceId, out Workspace? workspace))
		{
			return Result.FromException<Unit>(StoreExceptions.WorkspaceNotFound(WorkspaceId));
		}

		// Get the monitor.
		HMONITOR targetMonitorHandle =
			MonitorHandle == default ? rootSector.MonitorSector.ActiveMonitorHandle : MonitorHandle;

		Result<IMonitor> targetMonitorResult = ctx.Store.Pick(PickMonitorByHandle(targetMonitorHandle));
		if (!targetMonitorResult.TryGet(out IMonitor targetMonitor))
		{
			return Result.FromException<Unit>(targetMonitorResult.Error!);
		}

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = ctx.Store.Pick(PickWorkspaceByMonitor(targetMonitorHandle)).OrDefault();

		// Find the monitor which just lost `workspace`.
		IMonitor? sourceMonitor = ctx.Store.Pick(PickMonitorByWorkspace(WorkspaceId)).OrDefault();

		return ActivateWorkspace(ctx, rootSector, workspace, oldWorkspace, targetMonitor, sourceMonitor);
	}

	private Result<Unit> ActivateWorkspace(
		IContext ctx,
		MutableRootSector root,
		Workspace workspace,
		IWorkspace? oldWorkspace,
		IMonitor targetMonitor,
		IMonitor? sourceMonitor
	)
	{
		if (targetMonitor.Handle == sourceMonitor?.Handle)
		{
			Logger.Debug("Workspace is already activated");
			return Unit.Result;
		}

		MapSector mapSector = root.MapSector;

		mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(targetMonitor.Handle, WorkspaceId);

		if (sourceMonitor != null && oldWorkspace != null)
		{
			Logger.Debug($"Layout workspace {oldWorkspace} was previously in monitor {sourceMonitor}");

			mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(
				sourceMonitor.Handle,
				oldWorkspace.Id
			);
			ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(oldWorkspace.Id, FocusWindow: false));

			mapSector.QueueEvent(
				new MonitorWorkspaceChangedEventArgs()
				{
					Monitor = sourceMonitor,
					PreviousWorkspace = workspace,
					CurrentWorkspace = oldWorkspace
				}
			);
		}
		else if (oldWorkspace != null)
		{
			ctx.Store.Dispatch(new DeactivateWorkspaceTransform(oldWorkspace.Id));
		}

		ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(workspace.Id));
		mapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = targetMonitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = workspace
			}
		);

		return Unit.Result;
	}
}
