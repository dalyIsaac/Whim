namespace Whim;

/// <summary>
/// Set the name of the workspace with the provided <paramref name="Id"/> to <paramref name="Name"/>.
/// </summary>
/// <param name="Id">
/// The id of the workspace to set the name of.
/// </param>
/// <param name="Name">
/// The new name of the workspace.
/// </param>
public record SetWorkspaceNameTransform(WorkspaceId Id, string Name) : BaseWorkspaceTransform(Id)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		if (Name == workspace.BackingName)
		{
			return workspace;
		}

		Workspace newWorkspace = workspace with { BackingName = Name };
		rootSector.WorkspaceSector.QueueEvent(
			new WorkspaceRenamedEventArgs() { PreviousName = workspace.Name, Workspace = newWorkspace }
		);
		return newWorkspace;
	}
}
