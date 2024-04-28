using System;
using DotNext;

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
public record SetWorkspaceNameTransform(Guid Id, string Name) : BaseWorkspaceTransform(Id)
{
	private protected override Result<ImmutableWorkspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector sector,
		ImmutableWorkspace workspace
	)
	{
		ImmutableWorkspace newWorkspace = workspace with { Name = Name };
		sector.QueueEvent(new WorkspaceRenamedEventArgs() { PreviousName = workspace.Name, Workspace = newWorkspace });
		return newWorkspace;
	}
}
