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
	/// <param name="engine">
	/// The layout engine to check.
	/// </param>
	/// <param name="idx">
	/// The index of the layout engine in the workspace.
	/// </param>
	/// <param name="ctx">
	/// The context.
	/// </param>
	/// <returns></returns>
	public abstract bool Predicate(ILayoutEngine engine, int idx, IContext ctx);

	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		int layoutEngineIdx = workspace.LayoutEngines.GetMatchingIndex(Predicate, ctx);
		if (layoutEngineIdx == -1)
		{
			return Result.FromException<Workspace>(new WhimException("Provided layout engine not found"));
		}

		return WorkspaceUtils.SetActiveLayoutEngine(rootSector.WorkspaceSector, workspace, layoutEngineIdx);
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
	public override bool Predicate(ILayoutEngine engine, int idx, IContext ctx) => idx == LayoutEngineIndex;
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
	public override bool Predicate(ILayoutEngine engine, int idx, IContext ctx) =>
		engine == ctx.Store.Pick(PickActiveLayoutEngine(WorkspaceId)).Value;
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
	public override bool Predicate(ILayoutEngine engine, int idx, IContext ctx) => engine.Name == LayoutEngineName;
}
