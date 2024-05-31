using System;
using DotNext;

namespace Whim;

/// <summary>
/// Set the active layout engine in the workspace specified by <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace.
/// </param>
/// <param name="LayoutEnginePredicate">
/// A predicate which determines which layout engine should be activated.
/// </param>
public record ActivateLayoutEngineTransform(Guid WorkspaceId, Pred<ILayoutEngine> LayoutEnginePredicate)
	: BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		int layoutEngineIdx = workspace.LayoutEngines.GetMatchingIndex(LayoutEnginePredicate);
		if (layoutEngineIdx == -1)
		{
			return Result.FromException<Workspace>(new WhimException("Provided layout engine not found"));
		}

		return WorkspaceUtils.SetActiveLayoutEngine(rootSector.WorkspaceSector, workspace, layoutEngineIdx);
	}
}
