namespace Whim;

/// <summary>
/// Merges the windows of the given <paramref name="SourceWorkspaceId"/> into the given <paramref name="TargetWorkspaceId"/>.
/// </summary>
/// <param name="SourceWorkspaceId">The id of the workspace to remove.</param>
/// <param name="TargetWorkspaceId">The id of the workspace to merge the windows into.</param>
public record MergeWorkspaceWindowsTransform(WorkspaceId SourceWorkspaceId, WorkspaceId TargetWorkspaceId)
	: BaseWorkspaceTransform(TargetWorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace targetWorkspace
	)
	{
		MapSector sector = rootSector.MapSector;

		// Get the source workspace's windows.
		Result<IEnumerable<IWindow>> sourceWindowsResult = ctx.Store.Pick(PickWorkspaceWindows(SourceWorkspaceId));
		if (!sourceWindowsResult.TryGet(out IEnumerable<IWindow> sourceWindows))
		{
			return Result.FromError<Workspace>(sourceWindowsResult.Error!);
		}

		// Remove the workspace from the monitor map.
		HMONITOR monitor = sector.GetMonitorByWorkspace(SourceWorkspaceId);
		if (monitor != default)
		{
			sector.MonitorWorkspaceMap = sector.MonitorWorkspaceMap.SetItem(monitor, TargetWorkspaceId);
		}

		// Remap windows to the target workspace.
		foreach (IWindow window in sourceWindows)
		{
			sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.SetItem(window.Handle, TargetWorkspaceId);

			Result<bool> addWindowResult = ctx.Store.Dispatch(
				new AddWindowToWorkspaceTransform(TargetWorkspaceId, window)
			);
			if (!addWindowResult.IsSuccessful)
			{
				return Result.FromError<Workspace>(addWindowResult.Error!);
			}
		}

		return targetWorkspace;
	}
}
