namespace Whim;

/// <summary>
/// Focus the last focused window in the workspace with given <paramref name="WorkspaceId"/>.
///
/// NOTE: This does not update the workspace's <see cref="Workspace.LastFocusedWindow"/>.
/// Instead, it queues a call to <see cref="IWindow.Focus"/>. If there is no last focused window, the monitor's
/// desktop will be focused.
/// </summary>
/// <param name="WorkspaceId"></param>
public record FocusWorkspaceTransform(WorkspaceId WorkspaceId) : BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		if (workspace.LastFocusedWindowHandle != default)
		{
			rootSector.WorkspaceSector.WindowHandleToFocus = workspace.LastFocusedWindowHandle;
			return workspace;
		}

		Logger.Debug($"No windows in workspace {workspace.Name} to focus, focusing desktop");

		// Get the bounds of the monitor for this workspace.
		Result<IMonitor> monitorResult = ctx.Store.Pick(PickMonitorByWorkspace(workspace.Id));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Workspace>(monitorResult.Error!);
		}

		ctx.Store.WhimDispatch(new FocusMonitorDesktopTransform(monitor.Handle));
		return workspace;
	}
}
