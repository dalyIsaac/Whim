namespace Whim;

/// <summary>
/// Remove a workspace.
/// </summary>
public abstract record BaseRemoveWorkspaceTransform() : Transform
{
	/// <summary>
	/// Determines if the provided <paramref name="workspace"/> should be removed.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	public abstract bool ShouldRemove(Workspace workspace);

	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.WorkspaceSector;

		if (sector.Workspaces.Count - 1 < mutableRootSector.MonitorSector.Monitors.Length)
		{
			return Result.FromError<Unit>(new WhimError("There must be a workspace for each monitor"));
		}

		Workspace? workspaceToRemove = null;
		foreach (Workspace workspace in sector.Workspaces.Values)
		{
			if (ShouldRemove(workspace))
			{
				workspaceToRemove = workspace;
				break;
			}
		}

		if (workspaceToRemove is null)
		{
			return Result.FromError<Unit>(new WhimError("No matching workspace found"));
		}

		// Remove the workspace
		sector.WorkspaceOrder = sector.WorkspaceOrder.Remove(workspaceToRemove.Id);
		sector.Workspaces = sector.Workspaces.Remove(workspaceToRemove.Id);

		// Queue events
		WorkspaceId targetWorkspaceId = sector.WorkspaceOrder[^1];
		ctx.Store.Dispatch(new MergeWorkspaceWindowsTransform(workspaceToRemove.Id, targetWorkspaceId));
		ctx.Store.Dispatch(new ActivateWorkspaceTransform(targetWorkspaceId));

		sector.QueueEvent(new WorkspaceRemovedEventArgs() { Workspace = workspaceToRemove });
		sector.WorkspacesToLayout.Remove(workspaceToRemove.Id);

		return Unit.Result;
	}
}

/// <summary>
/// Removes the first workspace which matches the <paramref name="Id"/>.
/// </summary>
/// <param name="Id"></param>
public record RemoveWorkspaceByIdTransform(WorkspaceId Id) : BaseRemoveWorkspaceTransform()
{
	/// <inheritdoc />
	public override bool ShouldRemove(Workspace workspace) => workspace.Id == Id;
}

/// <summary>
/// Removes the first workspace which matches the <paramref name="Name"/>.
/// </summary>
/// <param name="Name"></param>
public record RemoveWorkspaceByNameTransform(string Name) : BaseRemoveWorkspaceTransform()
{
	/// <inheritdoc />
	public override bool ShouldRemove(Workspace workspace) => workspace.Name == Name;
}
