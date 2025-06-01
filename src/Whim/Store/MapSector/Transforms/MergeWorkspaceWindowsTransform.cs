namespace Whim;

/// <summary>
/// Merges the windows of the given <paramref name="SourceWorkspaceId"/> into the given <paramref name="TargetWorkspaceId"/>.
/// </summary>
/// <param name="SourceWorkspaceId">The id of the workspace to remove.</param>
/// <param name="TargetWorkspaceId">The id of the workspace to merge the windows into.</param>
public record MergeWorkspaceWindowsTransform(WorkspaceId SourceWorkspaceId, WorkspaceId TargetWorkspaceId) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector sector = rootSector.MapSector;

		// Get the workspaces.
		Result<IWorkspace> sourceWorkspace = ctx.Store.Pick(PickWorkspaceById(SourceWorkspaceId));
		if (!sourceWorkspace.TryGet(out IWorkspace Source))
		{
			return Result.FromError<Unit>(sourceWorkspace.Error!);
		}

		Result<IWorkspace> targetWorkspace = ctx.Store.Pick(PickWorkspaceById(TargetWorkspaceId));
		if (!targetWorkspace.TryGet(out IWorkspace Target))
		{
			return Result.FromError<Unit>(targetWorkspace.Error!);
		}

		// Remove the workspace from the monitor map.
		HMONITOR monitor = sector.GetMonitorByWorkspace(SourceWorkspaceId);
		if (monitor != default)
		{
			sector.MonitorWorkspaceMap = sector.MonitorWorkspaceMap.SetItem(monitor, TargetWorkspaceId);
		}

		// Remap windows to the first workspace which isn't active.
		foreach (IWindow window in Source.Windows)
		{
			sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.SetItem(window.Handle, TargetWorkspaceId);
		}

		foreach (IWindow window in Source.Windows)
		{
			Target.AddWindow(window);
		}

		return Unit.Result;
	}
}
