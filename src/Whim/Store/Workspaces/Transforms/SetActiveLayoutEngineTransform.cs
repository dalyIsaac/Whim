using System;
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
	Predicate<ImmutableWorkspace> WorkspacePredicate,
	Predicate<ILayoutEngine> LayoutEnginePredicate
) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		int workspaceIdx = slice.Workspaces.FindIndex(WorkspacePredicate);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(new WhimException("Workspace not found"));
		}

		ImmutableWorkspace workspace = slice.Workspaces[workspaceIdx];
		int layoutEngineIdx = workspace.LayoutEngines.FindIndex(LayoutEnginePredicate);
		if (layoutEngineIdx == -1)
		{
			return Result.FromException<Empty>(new WhimException("Layout engine not found"));
		}

		int previousLayoutEngineIdx = workspace.ActiveLayoutEngineIndex;
		slice.Workspaces = slice.Workspaces.SetItem(
			workspaceIdx,
			workspace with
			{
				ActiveLayoutEngineIndex = layoutEngineIdx
			}
		);

		// TODO: Queue DoLayout

		slice.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				PreviousLayoutEngine = workspace.LayoutEngines[previousLayoutEngineIdx],
				CurrentLayoutEngine = workspace.LayoutEngines[layoutEngineIdx]
			}
		);

		return Empty.Result;
	}
}
