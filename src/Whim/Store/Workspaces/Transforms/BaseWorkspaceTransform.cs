using System;
using DotNext;

namespace Whim;

/// <summary>
/// Base transform for a workspace operation.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="SkipDoLayout"></param>
public abstract record BaseWorkspaceTransform(Guid WorkspaceId, bool SkipDoLayout = false) : Transform<bool>
{
	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="sector">
	/// The workspace sector.
	/// </param>
	/// <param name="workspace">
	/// The workspace.
	/// </param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	private protected abstract Result<ImmutableWorkspace> WorkspaceOperation(
		WorkspaceSector sector,
		ImmutableWorkspace workspace
	);

	internal override Result<bool> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.Workspaces;

		int workspaceIdx = -1;
		for (int idx = 0; idx < sector.Workspaces.Count; idx++)
		{
			if (sector.Workspaces[idx].Id == WorkspaceId)
			{
				workspaceIdx = idx;
				break;
			}
		}

		if (workspaceIdx == -1)
		{
			return Result.FromException<bool>(StoreExceptions.WorkspaceNotFound());
		}

		ImmutableWorkspace workspace = sector.Workspaces[workspaceIdx];

		Result<ImmutableWorkspace> newWorkspaceResult = WorkspaceOperation(sector, workspace);
		if (!newWorkspaceResult.TryGet(out ImmutableWorkspace newWorkspace))
		{
			return Result.FromException<bool>(newWorkspaceResult.Error!);
		}

		if (newWorkspace == workspace)
		{
			return Result.FromValue(false);
		}

		sector.Workspaces = sector.Workspaces.SetItem(workspaceIdx, newWorkspace);

		if (SkipDoLayout == false)
		{
			sector.WorkspacesToLayout.Add(workspace.Id);
		}

		return Result.FromValue(true);
	}
}
