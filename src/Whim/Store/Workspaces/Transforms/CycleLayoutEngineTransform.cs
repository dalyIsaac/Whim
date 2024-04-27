using DotNext;

namespace Whim;

/// <summary>
/// Cycle the layout engine for the provided <paramref name="Workspace"/>
/// </summary>
/// <param name="Workspace">
/// The workspace to cycle the layout engine for.
/// </param>
/// <param name="Reverse">
/// Whether to cycle the layout engine in reverse.
/// </param>
public record CycleLayoutEngineTransform(ImmutableWorkspace Workspace, bool Reverse = false) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.Workspaces;

		int workspaceIdx = sector.Workspaces.IndexOf(Workspace);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		int delta = Reverse ? -1 : 1;
		int layoutEngineIdx = (Workspace.ActiveLayoutEngineIndex + delta).Mod(Workspace.LayoutEngines.Count);

		WorkspaceUtils.SetActiveLayoutEngine(sector, workspaceIdx, layoutEngineIdx);
		return Empty.Result;
	}
}
