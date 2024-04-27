using DotNext;

namespace Whim;

/// <summary>
/// Set the active layout engine in the <paramref name="Workspace"/>.
/// </summary>
/// <param name="Workspace">
/// The workspace to set the active layout engine for.
/// </param>
/// <param name="LayoutEnginePredicate">
/// A predicate which determines which layout engine should be activated.
/// </param>
public record SetActiveLayoutEngineTransform(ImmutableWorkspace Workspace, Pred<ILayoutEngine> LayoutEnginePredicate)
	: Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		int workspaceIdx = slice.Workspaces.IndexOf(Workspace);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		int layoutEngineIdx = Workspace.LayoutEngines.GetMatchingIndex(LayoutEnginePredicate);
		if (layoutEngineIdx == -1)
		{
			return Result.FromException<Empty>(new WhimException("Provided layout engine not found"));
		}

		WorkspaceUtils.SetActiveLayoutEngine(slice, workspaceIdx, layoutEngineIdx);
		return Empty.Result;
	}
}
