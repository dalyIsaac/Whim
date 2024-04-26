using DotNext;

namespace Whim;

/// <summary>
/// Set the active layout engine in the workspace with the provided <paramref name="WorkspacePredicate"/> to <paramref name="LayoutEnginePredicate"/>.
/// </summary>
/// <param name="WorkspacePredicate">
/// A predicate which determines which workspace should be affected.
/// </param>
/// <param name="LayoutEnginePredicate">
/// A predicate which determines which layout engine should be activated.
/// </param>
public record SetActiveLayoutEngineTransform(
	Pred<ImmutableWorkspace> WorkspacePredicate,
	Pred<ILayoutEngine> LayoutEnginePredicate
) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		int workspaceIdx = slice.Workspaces.GetMatchingIndex(WorkspacePredicate);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		ImmutableWorkspace workspace = slice.Workspaces[workspaceIdx];

		int layoutEngineIdx = workspace.LayoutEngines.GetMatchingIndex(LayoutEnginePredicate);
		if (layoutEngineIdx == -1)
		{
			return Result.FromException<Empty>(new WhimException("Provided layout engine not found"));
		}

		WorkspaceUtils.SetActiveLayoutEngine(slice, workspaceIdx, layoutEngineIdx);
		return Empty.Result;
	}
}
