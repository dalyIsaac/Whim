using DotNext;

namespace Whim;

/// <summary>
/// Base transform for a workspace operation.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="SkipDoLayout"></param>
public abstract record BaseWorkspaceTransform(WorkspaceId WorkspaceId, bool SkipDoLayout = false) : Transform<bool>
{
	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="workspaceSector"></param>
	/// <param name="workspace"></param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	private protected abstract Result<ImmutableWorkspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector workspaceSector,
		ImmutableWorkspace workspace
	);

	internal override Result<bool> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.WorkspaceSector;

		if (!sector.Workspaces.TryGetValue(WorkspaceId, out ImmutableWorkspace? workspace))
		{
			return Result.FromException<bool>(StoreExceptions.WorkspaceNotFound(WorkspaceId));
		}

		Result<ImmutableWorkspace> newWorkspaceResult = WorkspaceOperation(ctx, internalCtx, sector, workspace);
		if (!newWorkspaceResult.TryGet(out ImmutableWorkspace newWorkspace))
		{
			return Result.FromException<bool>(newWorkspaceResult.Error!);
		}

		if (newWorkspace == workspace)
		{
			return Result.FromValue(false);
		}

		sector.Workspaces = sector.Workspaces.SetItem(workspace.Id, newWorkspace);

		if (!SkipDoLayout)
		{
			sector.WorkspacesToLayout.Add(workspace.Id);
		}

		return Result.FromValue(true);
	}
}
