namespace Whim;

/// <summary>
/// Set the active layout engine in the workspace specified by <paramref name="WorkspaceId"/> and <see cref="Predicate"/>.
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace.
/// </param>
public abstract record ActivateLayoutEngineTransform(WorkspaceId WorkspaceId) : BaseWorkspaceTransform(WorkspaceId)
{
	/// <summary>
	/// The predicate to determine which layout engine should be activated.
	/// </summary>
	/// <param name="ctx">
	/// The context.
	/// </param>
	/// <param name="workspace">
	/// The workspace to check.
	/// </param>
	/// <param name="engine">
	/// The layout engine to check.
	/// </param>
	/// <param name="idx">
	/// The index of the layout engine in the workspace.
	/// </param>
	/// <returns></returns>
	public abstract bool Predicate(IContext ctx, Workspace workspace, ILayoutEngine engine, int idx);

	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			ILayoutEngine engine = workspace.LayoutEngines[idx];
			if (Predicate(ctx, workspace, engine, idx))
			{
				return WorkspaceUtils.SetActiveLayoutEngine(rootSector.WorkspaceSector, workspace, idx);
			}
		}

		return Result.FromError<Workspace>(new WhimError("Provided layout engine not found"));
	}
}

/// <summary>
/// Set the active layout engine in the workspace specified by <paramref name="WorkspaceId"/> and <paramref name="LayoutEngineIndex"/>.
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to set the layout engine in.
/// </param>
/// <param name="LayoutEngineIndex">
/// The index of the layout engine to set as active.
/// </param>
public record SetLayoutEngineFromIndexTransform(WorkspaceId WorkspaceId, int LayoutEngineIndex)
	: ActivateLayoutEngineTransform(WorkspaceId)
{
	/// <inheritdoc/>
	public override bool Predicate(IContext ctx, Workspace workspace, ILayoutEngine engine, int idx) =>
		idx == LayoutEngineIndex;
}

/// <summary>
/// Activate the previously active layout engine in the workspace specified by <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to activate the previously active layout engine in.
/// </param>
public record ActivatePreviouslyActiveLayoutEngineTransform(WorkspaceId WorkspaceId)
	: ActivateLayoutEngineTransform(WorkspaceId)
{
	/// <inheritdoc/>
	public override bool Predicate(IContext ctx, Workspace workspace, ILayoutEngine engine, int idx) =>
		idx == workspace.PreviousLayoutEngineIndex;
}

/// <summary>
/// Set the active layout engine in the workspace specified by <paramref name="WorkspaceId"/> and <paramref name="LayoutEngineName"/>.
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to set the layout engine in.
/// </param>
/// <param name="LayoutEngineName">
/// The name of the layout engine to set as active.
/// </param>
public record SetLayoutEngineFromNameTransform(WorkspaceId WorkspaceId, string LayoutEngineName)
	: ActivateLayoutEngineTransform(WorkspaceId)
{
	/// <inheritdoc/>
	public override bool Predicate(IContext ctx, Workspace workspace, ILayoutEngine engine, int idx) =>
		engine.Name == LayoutEngineName;
}
