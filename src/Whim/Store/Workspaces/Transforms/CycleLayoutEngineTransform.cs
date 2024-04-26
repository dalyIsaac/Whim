using DotNext;

namespace Whim;

/// <summary>
/// Cycle the layout engine for the workspace with the provided <paramref name="WorkspacePredicate"/>.
/// </summary>
/// <param name="WorkspacePredicate"></param>
/// <param name="Reverse">
/// Whether to cycle the layout engine in reverse.
/// </param>
public record CycleLayoutEngineTransform(Pred<ImmutableWorkspace> WorkspacePredicate, bool Reverse = false) : Transform
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

		int delta = Reverse ? -1 : 1;
		int layoutEngineIdx = (workspace.ActiveLayoutEngineIndex + delta).Mod(workspace.LayoutEngines.Count);

		WorkspaceUtils.SetActiveLayoutEngine(slice, workspaceIdx, layoutEngineIdx);
		return Empty.Result;
	}
}
