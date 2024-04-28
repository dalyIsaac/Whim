using System;
using DotNext;

namespace Whim;

/// <summary>
/// Cycle the layout engine for the provided workspace with the given <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to cycle the layout engine for.
/// </param>
/// <param name="Reverse">
/// Whether to cycle the layout engine in reverse.
/// </param>
public record CycleLayoutEngineTransform(Guid WorkspaceId, bool Reverse = false) : BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<ImmutableWorkspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector sector,
		ImmutableWorkspace workspace
	)
	{
		int delta = Reverse ? -1 : 1;
		int layoutEngineIdx = (workspace.ActiveLayoutEngineIndex + delta).Mod(workspace.LayoutEngines.Count);

		return WorkspaceUtils.SetActiveLayoutEngine(sector, workspace, layoutEngineIdx);
	}
}
