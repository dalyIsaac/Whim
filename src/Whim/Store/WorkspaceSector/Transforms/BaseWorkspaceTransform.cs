namespace Whim;

/// <summary>
/// Base transform for a workspace operation. The return value from the store dispatch is whether
/// the workspace has changed.
/// </summary>
/// <param name="WorkspaceId">
/// Defaults to the active workspace.
/// </param>
/// <param name="SkipDoLayout"></param>
public abstract record BaseWorkspaceTransform(WorkspaceId WorkspaceId, bool SkipDoLayout = false) : Transform<bool>
{
	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="rootSector"></param>
	/// <param name="workspace"></param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	private protected abstract Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	);

	internal override Result<bool> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.WorkspaceSector;
		WorkspaceId workspaceId = WorkspaceId.OrActiveWorkspace(ctx);

	if (!sector.Workspaces.TryGetValue(workspaceId, out Workspace? workspace))
		{
			return new Result<bool>(StoreExceptions.WorkspaceNotFound(workspaceId));
		}

		Result<Workspace> newWorkspaceResult = WorkspaceOperation(ctx, internalCtx, rootSector, workspace);
		if (!newWorkspaceResult.TryGet(out Workspace newWorkspace))
		{
			return new Result<bool>(newWorkspaceResult.Error!);
		}

		if (newWorkspace == workspace)
		{
			Logger.Debug("Workspace did not change from operation");
			return Result.FromValue(false);
		}

		sector.Workspaces = sector.Workspaces.SetItem(workspace.Id, newWorkspace);

		if (!SkipDoLayout)
		{
			sector.WorkspacesToLayout = sector.WorkspacesToLayout.Add(workspace.Id);
		}

		return Result.FromValue(true);
	}
}
