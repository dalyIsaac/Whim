namespace Whim;

/// <summary>
/// Focus the provided <paramref name="Window"/> in the workspace with given <paramref name="WorkspaceId"/>.
/// If <paramref name="Window"/> is <c>null</c>, focus the last focused window.
///
/// NOTE: This does not update the workspace's <see cref="Workspace.LastFocusedWindow"/>.
/// Instead, it queues a call to <see cref="IWindow.Focus"/>. If there is no last focused window, the monitor's
/// desktop will be focused.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
public record FocusWindowTransform(WorkspaceId WorkspaceId, IWindow? Window = null)
	: BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		IWindow? lastFocusedWindow = Window ?? workspace.LastFocusedWindow;
		if (lastFocusedWindow != null && !lastFocusedWindow.IsMinimized)
		{
			rootSector.WorkspaceSector.WindowHandleToFocus = lastFocusedWindow.Handle;
			return workspace;
		}

		Logger.Debug($"No windows in workspace {workspace.Name} to focus, focusing desktop");

		// Get the bounds of the monitor for this workspace.
		Result<IMonitor> monitorResult = ctx.Store.Pick(PickMonitorByWorkspace(workspace.Id));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Workspace>(monitorResult.Error!);
		}

		ctx.Store.Dispatch(new FocusMonitorDesktopTransform(monitor.Handle));
		return workspace;
	}
}
