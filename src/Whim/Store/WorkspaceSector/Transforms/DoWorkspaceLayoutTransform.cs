using System.Linq;

namespace Whim;

/// <summary>
/// Triggers a layout (sets all the window positions) for the workspace specified by <see cref="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="FocusWindow">Whether or not to focus the last focused window in the workspace.</param>
internal record DoWorkspaceLayoutTransform(WorkspaceId WorkspaceId, bool FocusWindow = true) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		if (!rootSector.WorkspaceSector.Workspaces.TryGetValue(WorkspaceId, out Workspace? workspace))
		{
			return Result.FromException<Unit>(StoreExceptions.WorkspaceNotFound(WorkspaceId));
		}

		WorkspaceSector sector = rootSector.WorkspaceSector;
		sector.WorkspacesToLayout = sector.WorkspacesToLayout.Add(WorkspaceId);

		if (!FocusWindow)
		{
			return Unit.Result;
		}

		// Focus the last focused window if there is one.
		sector.WindowHandleToFocus = workspace.LastFocusedWindowHandle;

		// If there isn't one, focus _a_ window.
		if (workspace.LastFocusedWindowHandle == default)
		{
			sector.WindowHandleToFocus = workspace.WindowPositions.Keys.FirstOrDefault();
		}

		// If there aren't any windows, focus the monitor.
		if (workspace.WindowPositions.Count == 0)
		{
			// return ctx.Store.Dispatch(new FocusMonitorDesktopTransform())
			Result<IMonitor> monitorResult = ctx.Store.Pick(PickMonitorByWorkspace(WorkspaceId));
			if (!monitorResult.TryGet(out IMonitor monitor))
			{
				return Result.FromException<Unit>(monitorResult.Error!);
			}

			return ctx.Store.Dispatch(new FocusMonitorDesktopTransform(monitor.Handle));
		}

		return Unit.Result;
	}
}
