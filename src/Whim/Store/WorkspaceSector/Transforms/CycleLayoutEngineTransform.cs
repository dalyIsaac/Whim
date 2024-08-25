namespace Whim;

/// <summary>
/// Cycle the layout engine for the provided workspace with the given <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to cycle the layout engine for. Defaults to the active workspace.
/// </param>
/// <param name="Reverse">
/// Whether to cycle the layout engine in reverse.
/// </param>
public record CycleLayoutEngineTransform(WorkspaceId WorkspaceId = default, bool Reverse = false)
	: BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		int delta = Reverse ? -1 : 1;
		int layoutEngineIdx = (workspace.ActiveLayoutEngineIndex + delta).Mod(workspace.LayoutEngines.Count);

		return WorkspaceUtils.SetActiveLayoutEngine(rootSector.WorkspaceSector, workspace, layoutEngineIdx);
	}
}
