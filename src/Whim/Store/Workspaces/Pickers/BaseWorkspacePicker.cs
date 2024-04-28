using System;
using DotNext;

namespace Whim;

/// <summary>
/// Base picker for retrieving something from a workspace.
/// </summary>
/// <typeparam name="TResult"></typeparam>
/// <param name="WorkspaceId"></param>
public abstract record BaseWorkspacePicker<TResult>(Guid WorkspaceId) : Picker<Result<TResult>>
{
	/// <summary>
	/// The operation to perform with the retrieved workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	protected abstract TResult Operation(ImmutableWorkspace workspace);

	internal override Result<TResult> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		IWorkspaceSector sector = rootSector.Workspaces;

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
			return Result.FromException<TResult>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		return Operation(sector.Workspaces[workspaceIdx]);
	}
}
